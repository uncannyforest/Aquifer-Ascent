using UnityEngine;

public class PanController : MonoBehaviour {
    public bool XY = true;
    public float speed = .25f;
    public Transform cameraTransform;

    private Camera camConfig;

    void Start() {
        camConfig = GetComponentInChildren<Camera>();
    }

    void Update() {
        float h = SimpleInput.GetAxisRaw("Horizontal");
        float v = SimpleInput.GetAxisRaw("Vertical");

        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        transform.position += (v * (XY ? Vector3.up : camForward) + h * cameraTransform.right)
             * speed * Time.unscaledDeltaTime * camConfig.orthographicSize;
    }
}
