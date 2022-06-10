using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityStandardAssets.Cameras;
using System;

[ExecuteInEditMode]
public class CameraMux : MonoBehaviour {
    public float defaultTransitionDuration;
    public Camera orthoCam;
    public Camera perspectiveCam;
    public bool defaultIsOrtho = true;

    private Camera activeCamera;
    private GameObject activeRig;

    private GameObject orthoRig;
    private GameObject perspectiveRig;
    // private Vector3 orthoPos;
    // private Quaternion orthoRot;
    // private Matrix4x4 orthoProj;
    // private Vector3 perspectivePos;
    // private Quaternion perspectiveRot;
    // private Matrix4x4 perspectiveProj;

    private IEnumerator coroutine;

    [System.Serializable] public class CameraEvent : UnityEvent<Camera> {}
    [SerializeField] CameraEvent onSwitchCamera;

    void Start() {
        activeCamera = defaultIsOrtho ? orthoCam : perspectiveCam;

        orthoRig = orthoCam.transform.parent.parent.gameObject;
        // orthoPos = orthoCam.transform.localPosition;
        // orthoRot = orthoCam.transform.localRotation;
        // orthoProj = activeCamera.projectionMatrix;
        perspectiveRig = perspectiveCam.transform.parent.parent.parent.gameObject;
        // perspectivePos = perspectiveCam.transform.localPosition;
        // perspectiveRot = perspectiveCam.transform.localRotation;
        // perspectiveProj = activeCamera.projectionMatrix;

        activeRig = defaultIsOrtho ? orthoRig : perspectiveRig;
    }

    private IEnumerator LerpFromTo(Matrix4x4 srcMat, Matrix4x4 destMat,
            Vector3 srcPos, Vector3 destPos, Quaternion srcRot, Quaternion destRot,  float duration) {
        float startTime = Time.time;
        try {
            while (Time.time - startTime < duration) {
                float lerp = SineInterpolate((Time.time - startTime) / duration);
                float rotLerp = (activeCamera == orthoCam) ? lerp * (2 - lerp) : lerp * lerp;
                float posLerp = lerp * (2 - lerp);
                float projLerp = (activeCamera == orthoCam) ? lerp * lerp : lerp * (2 - lerp);
                activeCamera.projectionMatrix = MatrixLerp(srcMat, destMat, projLerp);
                activeCamera.transform.localPosition = Vector3.Lerp(srcPos, destPos, posLerp);
                activeCamera.transform.localRotation = Quaternion.Lerp(srcRot, destRot, rotLerp);
                yield return 1;
            }
        } finally {
            activeCamera.projectionMatrix = destMat;
            activeCamera.transform.localPosition = destPos;
            activeCamera.transform.localRotation = destRot;
        }
    }
 
    public void SwitchCamera(bool ortho) {
        SwitchCamera(ortho, defaultTransitionDuration);
    }

    public void SwitchCamera(bool ortho, float duration) {
        if (!ortho && activeCamera == orthoCam) {
                perspectiveRig.GetComponent<MixedAutoCam>().YRotation = activeCamera.transform.rotation.eulerAngles.y;
        }

        Camera newCamera = ortho ? orthoCam : perspectiveCam;
        GameObject newRig = ortho ? orthoRig : perspectiveRig;
        SwitchCamera(newCamera, newRig, duration);
    }

    public void SwitchCamera(Camera newCamera, GameObject newRig, float duration) {
        if (newCamera == activeCamera) return;
        if (coroutine != null) {
            StopCoroutine(coroutine);
            ((IDisposable)coroutine).Dispose();
        }
        Matrix4x4 oldMatrix = activeCamera.projectionMatrix;
        Matrix4x4 newMatrix = newCamera.projectionMatrix;
        Vector3 newPosition = newCamera.transform.localPosition;
        Quaternion newRotation = newCamera.transform.localRotation;
        newCamera.transform.position = activeCamera.transform.position;
        newCamera.transform.rotation = activeCamera.transform.rotation;
        activeRig.SetActive(false);
        newRig.SetActive(true);
        activeCamera = newCamera;
        activeRig = newRig;
        onSwitchCamera.Invoke(newCamera);
        coroutine = LerpFromTo(oldMatrix, newMatrix,
            activeCamera.transform.localPosition, newPosition, activeCamera.transform.localRotation, newRotation,
            duration);
            StartCoroutine(coroutine);
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
