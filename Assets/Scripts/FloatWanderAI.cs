using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatWanderAI : MonoBehaviour
{
    public float moveSpeed = 0.1f;
    public float updateTime = 0.5f;

    private bool alreadyMoving = false;
    private Vector3 direction = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (canMove()) {
            if (alreadyMoving == false) {
                StartCoroutine(Wander());
            }
        }
        if (canMove()) {
            transform.position += direction * moveSpeed * Time.deltaTime;
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

        direction += new Vector3(x, y, z);
        if (direction.magnitude > 1f) {
            direction /= direction.magnitude;
        }
    }

    bool canMove() {
        return !gameObject.GetComponent<PickMeUp>().IsHeld;
    }
}
