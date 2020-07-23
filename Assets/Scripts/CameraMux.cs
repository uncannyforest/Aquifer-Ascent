using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityStandardAssets.Cameras;

[ExecuteInEditMode]
public class CameraMux : MonoBehaviour {
    public float defaultTransitionDuration;
    public Camera orthoCam;
    public Camera perspectiveCam;
    public Camera defaultCamera;

    private Camera activeCamera;

    private GameObject orthoRig;
    private GameObject perspectiveRig;
    private Vector3 orthoPos;
    private Quaternion orthoRot;
    private Vector3 perspectivePos;
    private Quaternion perspectiveRot;

    [System.Serializable] public class CameraEvent : UnityEvent<Camera> {}
    [SerializeField] CameraEvent onSwitchCamera;

    void Start() {
        activeCamera = defaultCamera;

        orthoRig = orthoCam.transform.parent.parent.gameObject;
        orthoPos = orthoCam.transform.localPosition;
        orthoRot = orthoCam.transform.localRotation;
        perspectiveRig = perspectiveCam.transform.parent.parent.gameObject;
        perspectivePos = perspectiveCam.transform.localPosition;
        perspectiveRot = perspectiveCam.transform.localRotation;
    }

    private IEnumerator LerpFromTo(Matrix4x4 srcMat, Matrix4x4 destMat,
            Vector3 srcPos, Vector3 destPos, Quaternion srcRot, Quaternion destRot, float duration) {
        float startTime = Time.time;
        while (Time.time - startTime < duration) {
            float lerp = SineInterpolate((Time.time - startTime) / duration);
            float rotLerp = (activeCamera == orthoCam) ? lerp * (2 - lerp) : lerp * lerp;
            float posLerp = lerp * (2 - lerp);
            float projLerp = (activeCamera == orthoCam) ? lerp * lerp : lerp * (2 - lerp);
            activeCamera.projectionMatrix = MatrixLerp(srcMat, destMat, lerp);
            activeCamera.transform.localPosition = Vector3.Lerp(srcPos, destPos, lerp);
            activeCamera.transform.localRotation = Quaternion.Lerp(srcRot, destRot, rotLerp);
            yield return 1;
        }
        activeCamera.projectionMatrix = destMat;
        activeCamera.transform.localPosition = destPos;
        activeCamera.transform.localRotation = destRot;
    }
 
    public Coroutine SwitchCamera(bool ortho) {
        StopAllCoroutines();
        Camera newCamera = ortho ? orthoCam : perspectiveCam;

        if (!ortho) {
            perspectiveRig.GetComponent<MixedAutoCam>().YRotation = orthoCam.transform.rotation.eulerAngles.y;
        }

        Matrix4x4 oldMatrix = activeCamera.projectionMatrix;
        Matrix4x4 newMatrix = newCamera.projectionMatrix;
        Vector3 newPosition = ortho ? orthoPos : perspectivePos;
        Quaternion newRotation = ortho ? orthoRot : perspectiveRot;
        newCamera.transform.position = activeCamera.transform.position;
        newCamera.transform.rotation = activeCamera.transform.rotation;
        (ortho ? perspectiveRig : orthoRig).SetActive(false);
        (ortho ? orthoRig : perspectiveRig).SetActive(true);
        activeCamera = newCamera;
        onSwitchCamera.Invoke(newCamera);
        return StartCoroutine(LerpFromTo(oldMatrix, newMatrix, activeCamera.transform.localPosition,
            newPosition, activeCamera.transform.localRotation, newRotation, defaultTransitionDuration));
    }

    private Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float t) {
         Matrix4x4 result = new Matrix4x4();
         result.SetRow(0, Vector4.Lerp(from.GetRow(0), to.GetRow(0), t));
         result.SetRow(1, Vector4.Lerp(from.GetRow(1), to.GetRow(1), t));
         result.SetRow(2, Vector4.Lerp(from.GetRow(2), to.GetRow(2), t));
         result.SetRow(3, Vector4.Lerp(from.GetRow(3), to.GetRow(3), t));
         return result;
     }

     private float SineInterpolate(float t) {
         return Mathf.Sin((t - 0.5f) * Mathf.PI) / 2 + 0.5f;
     }
}
