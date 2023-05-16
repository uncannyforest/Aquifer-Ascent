using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(Holdable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Bridge : MonoBehaviour {
    public float holdAngle = 60f;
    public float dropDisplacement = .5f;
    public float dropTorque = 1f;
    public float maxStationarySpeed = .01f;
    public float mountYTolerance = .25f;
    public float unmountTopDistance = 0f;
    public float unmountBottomDistance = .1f;
    public bool placed = false;
    public GameObject walls;

    private Rigidbody myRigidbody;
    private BoxCollider myCollider;
    private float height;
    public float depth;
    private Transform model;
    private Transform player;

    void Start() {
        myRigidbody = GetComponent<Rigidbody>();
        myCollider = GetComponent<BoxCollider>();
        height = myCollider.bounds.size.y - unmountTopDistance;
        player = GameObject.FindObjectOfType<ThirdPersonCharacter>().transform;
        depth = myCollider.bounds.max.z - player.GetComponent<CapsuleCollider>().bounds.min.y;
        model = transform.GetChild(2);
    }

    void UpdateHeldState(float heldState) {
        transform.rotation = Quaternion.Lerp(player.rotation * Quaternion.Euler(30, 0, 0), player.rotation * Quaternion.Euler(0, 0, holdAngle), heldState);
        model.localPosition = Vector3.down * height / 2 * heldState;

        if (heldState == 0f) StartCoroutine(Place());
        else {
            StopAllCoroutines();
            placed = false;
            myCollider.enabled = false;
            myRigidbody.isKinematic = true;
        }
    }

    private IEnumerator Place() {
        Debug.Log("Placing!");
        myCollider.enabled = true;
        myRigidbody.isKinematic = false;
        // Vector3 position = player.TransformPoint(new Vector3(0, 0, dropDisplacement));
        // myRigidbody.MovePosition(position);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        while (myRigidbody.velocity.magnitude > maxStationarySpeed) {
            yield return new WaitForFixedUpdate();
        }
        myRigidbody.isKinematic = true;
        placed = true;
    }

    void OnCollisionEnter(Collision collision) {
		if (placed && collision.gameObject.CompareTag("Player")) {
            GameObject player = collision.gameObject;
            Vector3 playerInLocalCoords = transform.InverseTransformPoint(player.transform.position);
            Vector3 contactPoint = transform.TransformPoint(Vector3.up * playerInLocalCoords.y);
            if (Mathf.Abs(contactPoint.y - player.transform.position.y) < mountYTolerance) {
                player.GetComponent<Rigidbody>().MovePosition(contactPoint);
                player.GetComponent<ThirdPersonUserControl>().moveOverride = MoveOverride;
                //walls.SetActive(true);
                Vector3 yDir = Vector3.ProjectOnPlane(transform.up, Vector3.up);
                walls.transform.rotation = Quaternion.LookRotation(yDir, Vector3.up);
                Debug.Log("Mounting bridge");
            }
        }
    }

    private Vector3 MoveOverride(ThirdPersonUserControl player, Vector3 move, bool jump) {
        if (jump) {
            Debug.Log("Unmounting bridge");
            player.moveOverride = null;
            walls.SetActive(false);
            player.GetComponent<ThirdPersonCharacter>().m_GroundGravity = 1;
            return move;
        } else {
            Vector3 newPlayerInLocalCoords = transform.InverseTransformPoint(player.transform.position + move * Time.deltaTime);
            Vector3 moveInLocalCoords = transform.InverseTransformVector(move);

            if (newPlayerInLocalCoords.y > height || newPlayerInLocalCoords.y < -unmountBottomDistance) {
                Debug.Log("Unmounting bridge off the " + (newPlayerInLocalCoords.y > height ? "end" : "beginning"));
                player.moveOverride = null;
                walls.SetActive(false);
                player.GetComponent<ThirdPersonCharacter>().m_GroundGravity = 1;
                return move;
            }
            if (moveInLocalCoords.y > 0) {
                player.GetComponent<ThirdPersonCharacter>().m_GroundGravity = .1f;
                Debug.DrawLine(player.transform.position, player.transform.position
                    + transform.TransformVector(Vector3.up * moveInLocalCoords.magnitude), Color.magenta);
                Debug.Log("Positive!");
                return transform.TransformVector(Vector3.up * moveInLocalCoords.magnitude);
            } else if (moveInLocalCoords.y < 0) {
                player.GetComponent<ThirdPersonCharacter>().m_GroundGravity = 1;
                Debug.DrawLine(player.transform.position, player.transform.position
                    + transform.TransformVector(Vector3.down * moveInLocalCoords.magnitude), Color.magenta);
                Debug.Log("Negative!");
                return transform.TransformVector(Vector3.down * moveInLocalCoords.magnitude);
            } else {
                return Vector3.zero;
            }
        }
    }
}