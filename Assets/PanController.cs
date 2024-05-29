using UnityEngine;

public class PanController : MonoBehaviour {
    public float speed = .25f;
    public Transform cameraTransform;

    void Update() {
        float h = SimpleInput.GetAxisRaw("Horizontal");
        float v = SimpleInput.GetAxisRaw("Vertical");

        Debug.Log("Hi" + h);

        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        transform.position += (v * camForward + h * cameraTransform.right) * speed;
    }
}
