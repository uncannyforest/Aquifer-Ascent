using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Holdable : MonoBehaviour
{
    public GameObject taggedCanPickUp;
    public string optionalAction = "";
    public string parentWhenFreeName = "Free Orbs";
    public AudioClip pickUpSound;
    public AudioClip setDownSound;
    public float pickUpTime = 0.5f;

    [NonSerialized] public Transform parentWhenFree;
    private float heldState = 0.0f; // 0 if not held, 1 if held
    private Transform playerHoldTransform;
    Collider physicsCollider;
    Rigidbody myRigidbody;
    Vector3 oldPosition;
    private AudioSource objectAudio; 
    private Bounds myColliderBounds;
    private bool isUsed;

    public bool isEverHoldable = true;
    private HashSet<String> cannotHold = new HashSet<String>();
    private string cannotHoldDebug = "";
    private bool IsHoldable {
        set {
            if (value) taggedCanPickUp.tag = "CanPickUp";
            else taggedCanPickUp.tag = "Untagged";
        }
    }
    public void CanHold(string key, bool toggle) {
        int oldCannotHoldCount = cannotHold.Count;
        if (toggle) cannotHold.Remove(key);
        else cannotHold.Add(key);
        cannotHoldDebug = cannotHold.Aggregate("", (acc, str) => acc + " " + str);
        if (oldCannotHoldCount + cannotHold.Count == 1) {
            IsHoldable = cannotHold.Count == 0;
        }
    }

    public bool IsHeld {
        get => this.transform.parent == playerHoldTransform;
        private set {
            if (value) {
                this.transform.parent = playerHoldTransform;
            } else {
                this.transform.parent = parentWhenFree;
            }
        }
    }

    public bool IsFree {
        get {
            if (parentWhenFree == null) {
                Debug.Log(gameObject.name + " missing parentWhenFree for now");
            }

            return this.transform.parent == parentWhenFree;
        }
    }

    void Start(){
        OnChangeActiveScene(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
        playerHoldTransform = GameObject.FindWithTag("Player").transform.Find("HoldLocation");
        physicsCollider = GetComponent<Collider>();
        myColliderBounds = physicsCollider.bounds;
        myRigidbody = GetComponent<Rigidbody>();
        objectAudio = GetComponent<AudioSource>();
        parentWhenFree = this.transform.parent;
        if (!isEverHoldable) CanHold("ever", false);
    }

    void OnEnable() {
        SceneManager.activeSceneChanged += OnChangeActiveScene;
    }
    
    void OnDisable() {
        SceneManager.activeSceneChanged -= OnChangeActiveScene;
    }

    // Update is called once per frame
    void Update(){
        if (IsHeld && heldState < 1f) {
            heldState += Time.deltaTime / pickUpTime;
            heldState = Mathf.Min(heldState, 1f);

            Vector3 newPosition = playerHoldTransform.position;
            this.transform.position =
                Vector3.Lerp(oldPosition, newPosition, QuadInterpolate(heldState));
            gameObject.SendMessage("UpdateHeldState", heldState);
        } else if (!IsHeld && heldState > 0f) {
            heldState -= Time.deltaTime / pickUpTime;
            heldState = Mathf.Max(heldState, 0f);
            gameObject.SendMessage("UpdateHeldState", heldState);
        }
    }

    public void Drop() {
        FinishDrop();
        playerHoldTransform.parent.GetComponent<HoldObject>().OnDropObject(gameObject, false);
    }

    public void FinishDrop() {
        IsHeld = false;
        if (!isUsed) {
            objectAudio.PlayOneShot(setDownSound, 0.5f);
            myRigidbody.isKinematic = false;
        }
    }

    public void Hold() {
        IsHeld = true;
        objectAudio.PlayOneShot(pickUpSound, 0.5f);
        myRigidbody.isKinematic = true;
        oldPosition = this.transform.position;
        this.transform.rotation = playerHoldTransform.rotation;
        playerHoldTransform.parent.GetComponent<HoldObject>().OnHoldObject(gameObject);
    }

    public void Use () {
        isUsed = true;
    }

    public void SetOptionalAction(string optionalAction) {
        this.optionalAction = optionalAction;
        playerHoldTransform.parent.GetComponent<HoldObject>().OnHoldObject(gameObject);
    }

    public float GetColliderWidth() {
        // this is broken out here becuase bounds can only be queried when collider is active
        return myColliderBounds.size.z;
    }

    private float QuadInterpolate(float x) {
        return -x * (x - 2);
    }

    void OnChangeActiveScene(Scene current, Scene next) {
        foreach (GameObject rootObject in next.GetRootGameObjects()) {
            if (rootObject.name == parentWhenFreeName) {
                parentWhenFree = rootObject.transform;
            }
        }
    }

}

