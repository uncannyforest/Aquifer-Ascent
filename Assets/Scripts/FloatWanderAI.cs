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

    Rigidbody rigidBody;

    void Start() {
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        if (canMove()) {
            if (alreadyMoving == false) {
                StartCoroutine(Wander());
            }
        }
    }

    IEnumerator Wander() {
        alreadyMoving = true;
        updateDirection();
        yield return new WaitForSeconds(updateTime);
        alreadyMoving = false;
    }

    void updateDirection() {
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

    bool canMove() {
        return !gameObject.GetComponent<Holdable>().IsHeld;
    }
}
