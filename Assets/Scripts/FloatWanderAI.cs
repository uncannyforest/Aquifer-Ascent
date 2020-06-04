using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatWanderAI : MonoBehaviour
{
    public float turnSpeed = 0.02f;
    public float maxMoveSpeed = 0.05f;
    public float updateTime = 0.5f;

    private bool alreadyMoving = false;
    private bool canMove = true;

    Rigidbody rigidBody;

    public bool CanMove {
        get => canMove && !gameObject.GetComponent<Holdable>().IsHeld;
        set {
            canMove = value;
            Debug.Log(gameObject.name + " WanderAI.CanMove set to " + value);
        }
    }

    void Start() {
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        if (CanMove) {
            if (alreadyMoving == false) {
                StartCoroutine(Wander());
            }
        }
    }

    IEnumerator Wander() {
        alreadyMoving = true;
        UpdateDirection();
        yield return new WaitForSeconds(updateTime);
        alreadyMoving = false;
    }

    void UpdateDirection() {
        // random point on sphere using equal area projection
        float theta = Random.Range(0f, 2 * Mathf.PI);
        float y = Random.Range(-1f, 1f);
        float x = Mathf.Sqrt(1 - y * y) * Mathf.Cos(theta);
        float z = Mathf.Sqrt(1 - y * y) * Mathf.Sin(theta);

        rigidBody.velocity += new Vector3(x, y, z) * turnSpeed;
        if (rigidBody.velocity.magnitude > maxMoveSpeed) {
            rigidBody.velocity = rigidBody.velocity.normalized * maxMoveSpeed;
        }
    }
}
