using UnityEngine;

public class IsoCamMovement : MonoBehaviour {
    public const float ISO_X = 35.2643897f; // arctan(1 / sqrt(2))

    public float hTurnSpeed = 1.5f;
    public float zoomSpeed = .03f;
    public float minCameraSize = 1f;
    public float defaultCameraSize = 2f;
    public float maxCameraSize = 4f;
    public float lightScaleFactor = 2;
    
    public float CameraSize {
        get => camConfig.orthographicSize;
        set {
            camConfig.orthographicSize = value;
        }
    }

    private Camera camConfig;
    private Light lightConfig;
    private bool overhead = false;

    void Awake() {
        camConfig = GetComponentInChildren<Camera>();
        lightConfig = GetComponentInChildren<Light>();
    }

    void Update() {
        HandleRotationMovement();
    }

    private float AngleClamp(float angle) {
        while (angle < -179) angle += 360;
        while (angle > 181) angle -= 360;
        return angle;
    }

    private void HandleRotationMovement() {
        float x = -SimpleInput.GetAxisRaw("Horizontal");
        float y = -SimpleInput.GetAxisRaw("Mouse X");
        overhead ^= SimpleInput.GetButtonDown("Jump");

        Vector3 transformEulers = transform.localRotation.eulerAngles;
        float lookAngle = transformEulers.y;
        lookAngle = AngleClamp(lookAngle);

        float direction = -x * hTurnSpeed * Time.unscaledDeltaTime;
        lookAngle += direction;
        lookAngle = AngleClamp(lookAngle);

        Quaternion transformTargetRot = Quaternion.Euler(overhead ? 90 : ISO_X, lookAngle, 0f);

        float diff = -y * zoomSpeed * CameraSize * Time.unscaledDeltaTime;
        CameraSize += diff; // multiply by cameraSize for more natural, log movement
        // and make sure the new value is within the size range
        CameraSize = Mathf.Clamp(CameraSize, minCameraSize, maxCameraSize);
        if (lightConfig != null) {
            lightConfig.range = CameraSize * lightScaleFactor;
            lightConfig.transform.localScale = CameraSize * Vector3.one;
        }

        transform.localRotation = transformTargetRot;
    }
}