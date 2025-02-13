using UnityEngine;

public class PanController : MonoBehaviour {
    public float speed = .25f;
    public Transform cameraTransform;

    private Camera camConfig;

    void Start() {
        camConfig = GetComponentInChildren<Camera>();
    }

    void Update() {
        float h = 0;//SimpleInput.GetAxisRaw("Horizontal");
        float f = SimpleInput.GetAxisRaw("Vertical");
        float v = SimpleInput.GetAxisRaw("Mouse Y");

        Vector3 camForward = Vector3.Scale(cameraTransform.forward + cameraTransform.up, new Vector3(1, 0, 1)).normalized;
        transform.position += (v * Vector3.up + f * camForward + h * cameraTransform.right)
             * speed * Time.unscaledDeltaTime * camConfig.orthographicSize;
    }
}
