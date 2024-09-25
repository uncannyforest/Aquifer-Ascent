using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Cameras;

/// <summary>FreeLookCam, minus follow player and not storing orientation state<summary>
[RequireComponent(typeof(ProtectCameraFromWallClip))]
[RequireComponent(typeof(MixedAutoCam))]
public class FreeLookCamMovement : MonoBehaviour
{
    // This script is designed to be placed on the root object of a camera rig,
    // comprising 3 gameobjects, each parented to the next:

    // 	Camera Rig
    // 		Pivot
    // 			Camera

    [SerializeField] private float m_HTurnSpeed = 1.5f;  // How fast the rig will rotate left-right from user input.
    [SerializeField] private float m_VTurnSpeed = .03f;   // How fast the rig will rotate up-down and forward-back from user input.
    [SerializeField] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
    [SerializeField] private bool m_LockCursor = false;                   // Whether the cursor should be hidden and locked.
    [SerializeField] private AnimationCurve m_TiltCurve = new AnimationCurve(
            new Keyframe(0, -45),
            new Keyframe(1, 0),  
            new Keyframe(2f, 7.5f), 
            new Keyframe(3, 10), 
            new Keyframe(5, 75));                
    [SerializeField] private AnimationCurve m_DistanceCurve = new AnimationCurve(
            new Keyframe(0, 1f),
            new Keyframe(1, 1f), 
            new Keyframe(2f, 2), 
            new Keyframe(3, 3.5f), 
            new Keyframe(5, 3.5f));
    [SerializeField] private float m_DistanceCurveConstantFactor = .5f; // rather than changing the whole curve . . .
    [SerializeField] private float m_DefaultTiltDistanceFrame = 2f;
    [SerializeField] private float m_ResetVTurnSpeed = .15f;
    
    public float m_TiltDistanceFrame;

    private Vector3 m_PivotEulers;
    private bool m_TiltDistanceFrameIsChanging = false;
    private bool m_IsResettingCamera = false;
    private ProtectCameraFromWallClip m_ClipScript;
    private MixedAutoCam m_AutoScript;

    public Transform m_Pivot; // the point at which the camera pivots around

    protected void Awake()
    {
        m_TiltDistanceFrame = m_DefaultTiltDistanceFrame;
        m_PivotEulers = m_Pivot.localRotation.eulerAngles;

        // Lock or unlock the cursor.
        Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !m_LockCursor;

        m_ClipScript = GetComponent<ProtectCameraFromWallClip>();
        m_AutoScript = GetComponent<MixedAutoCam>();
    }


    protected void Update()
    {
        if (SimpleInput.GetButtonDown("Reset Camera") && m_AutoScript.IsDoubleResetReady) {
            m_IsResettingCamera = true;
            m_TiltDistanceFrameIsChanging = true;
            m_ClipScript.maxDistanceIsChanging = true;
        }
        HandleRotationMovement();
        if (m_LockCursor && Input.GetMouseButtonUp(0))
        {
            Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !m_LockCursor;
        }
    }


    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleRotationMovement()
    {
        if(Time.timeScale < float.Epsilon)
        return;

        // Read the user input
        var x = SimpleInput.GetAxis("Mouse X");
        var y = SimpleInput.GetAxis("Mouse Y");

        if (y != 0 && !m_TiltDistanceFrameIsChanging) {
            m_TiltDistanceFrameIsChanging = true;
            gameObject.GetComponent<ProtectCameraFromWallClip>().maxDistanceIsChanging = true;
        } else if (y == 0 && m_TiltDistanceFrameIsChanging && !m_IsResettingCamera) {
            m_TiltDistanceFrameIsChanging = false;
            gameObject.GetComponent<ProtectCameraFromWallClip>().maxDistanceIsChanging = false;
        }

        // get current angle
        Vector3 m_TransformEulers = transform.localRotation.eulerAngles;
        float m_LookAngle = m_TransformEulers.y;

        // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
        m_LookAngle += x * m_HTurnSpeed * Time.deltaTime;

        // Rotate the rig (the root object) around Y axis only:
        Quaternion m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

        // we adjust the current angle based on Y mouse input and turn speed
        if (!m_IsResettingCamera) {
            m_TiltDistanceFrame -= y * m_VTurnSpeed * Time.deltaTime;
        } else {
            if (Mathf.Abs(m_TiltDistanceFrame - m_DefaultTiltDistanceFrame) < m_ResetVTurnSpeed) {
                m_IsResettingCamera = false;
                m_TiltDistanceFrameIsChanging = false;
                gameObject.GetComponent<ProtectCameraFromWallClip>().maxDistanceIsChanging = false;
            }
            m_TiltDistanceFrame = Mathf.MoveTowards(m_TiltDistanceFrame, m_DefaultTiltDistanceFrame, m_ResetVTurnSpeed);
        }
        // and make sure the new value is within the tilt range
        m_TiltDistanceFrame = Mathf.Clamp(m_TiltDistanceFrame, 0, m_TiltCurve.keys[m_TiltCurve.length-1].time);

        // Tilt input around X is applied to the pivot (the child of this object)
        Quaternion m_PivotTargetRot = Quaternion.Euler(
            m_TiltCurve.Evaluate(m_TiltDistanceFrame), m_PivotEulers.y , m_PivotEulers.z);
        // Tilt input around X is applied to the pivot (the child of this object)
        float m_TargetDistance = m_DistanceCurve.Evaluate(m_TiltDistanceFrame) * m_DistanceCurveConstantFactor;

        if (m_TurnSmoothing > 0)
        {
            m_Pivot.localRotation = Quaternion.Slerp(m_Pivot.localRotation, m_PivotTargetRot, m_TurnSmoothing * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * Time.deltaTime);
        }
        else
        {
            m_Pivot.localRotation = m_PivotTargetRot;
            transform.localRotation = m_TransformTargetRot;
        }

        gameObject.GetComponent<ProtectCameraFromWallClip>().maxDistance = m_TargetDistance;
    }
}