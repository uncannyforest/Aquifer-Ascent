using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Cameras;

/// <summary>FreeLookCam, minus follow player and not storing orientation state<summary>
[RequireComponent(typeof(ProtectCameraFromWallClip))]
public class FreeLookCamMovement : MonoBehaviour
{
    // This script is designed to be placed on the root object of a camera rig,
    // comprising 3 gameobjects, each parented to the next:

    // 	Camera Rig
    // 		Pivot
    // 			Camera

    [Range(0f, 10f)] [SerializeField] private float m_HTurnSpeed = 1.5f;  // How fast the rig will rotate left-right from user input.
    [Range(0f, 1f)] [SerializeField] private float m_VTurnSpeed = .03f;   // How fast the rig will rotate up-down and forward-back from user input.
    [SerializeField] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
    [SerializeField] private bool m_LockCursor = false;                   // Whether the cursor should be hidden and locked.
    [SerializeField] private AnimationCurve tiltCurve = new AnimationCurve(
            new Keyframe(0, -45),
            new Keyframe(1, 0),  
            new Keyframe(2f, 7.5f), 
            new Keyframe(3, 10), 
            new Keyframe(5, 75));                
    [SerializeField] private AnimationCurve distanceCurve = new AnimationCurve(
            new Keyframe(0, 1f),
            new Keyframe(1, 1f), 
            new Keyframe(2f, 2), 
            new Keyframe(3, 3.5f), 
            new Keyframe(5, 3.5f));
    [SerializeField] private float defaultTiltDistanceFrame = 2f;

    public float m_TiltDistanceFrame;

    private Vector3 m_PivotEulers;
    private bool m_TiltDistanceFrameIsChanging = false;

    protected Transform m_Cam; // the transform of the camera
    protected Transform m_Pivot; // the point at which the camera pivots around

    protected void Awake()
    {
        // find the camera in the object hierarchy
        m_Cam = GetComponentInChildren<Camera>().transform;
        m_Pivot = m_Cam.parent;

        m_TiltDistanceFrame = defaultTiltDistanceFrame;
        m_PivotEulers = m_Pivot.localRotation.eulerAngles;

        // Lock or unlock the cursor.
        Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !m_LockCursor;
    }


    protected void Update()
    {
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
        var x = CrossPlatformInputManager.GetAxis("Mouse X");
        var y = CrossPlatformInputManager.GetAxis("Mouse Y");

        if (y != 0 && !m_TiltDistanceFrameIsChanging) {
            m_TiltDistanceFrameIsChanging = true;
            gameObject.GetComponent<ProtectCameraFromWallClip>().maxDistanceIsChanging = true;
        } else if (y == 0 && m_TiltDistanceFrameIsChanging) {
            m_TiltDistanceFrameIsChanging = false;
            gameObject.GetComponent<ProtectCameraFromWallClip>().maxDistanceIsChanging = false;
        }

        // get current angle
        Vector3 m_TransformEulers = transform.localRotation.eulerAngles;
        float m_LookAngle = m_TransformEulers.y;

        // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
        m_LookAngle += x*m_HTurnSpeed;

        // Rotate the rig (the root object) around Y axis only:
        Quaternion m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

        // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
        m_TiltDistanceFrame -= y*m_VTurnSpeed;
        // and make sure the new value is within the tilt range
        m_TiltDistanceFrame = Mathf.Clamp(m_TiltDistanceFrame, 0, tiltCurve.keys[tiltCurve.length-1].time);

        // Tilt input around X is applied to the pivot (the child of this object)
        Quaternion m_PivotTargetRot = Quaternion.Euler(
            tiltCurve.Evaluate(m_TiltDistanceFrame), m_PivotEulers.y , m_PivotEulers.z);
        // Tilt input around X is applied to the pivot (the child of this object)
        float m_TargetDistance = distanceCurve.Evaluate(m_TiltDistanceFrame);

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