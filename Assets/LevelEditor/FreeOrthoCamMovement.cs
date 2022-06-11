using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Cameras;

public class FreeOrthoCamMovement : MonoBehaviour {
    [Range(0f, 10f)] [SerializeField] private float moveSpeed = 1f;
    [Range(0f, 10f)] [SerializeField] private float m_HTurnSpeed = 1.5f;  // How fast the rig will rotate left-right from user input.
    [SerializeField] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
    
    private bool rotateKeyDown = false;
    private float m_AimingForIso = 0;

    protected void Update() {
        HandleMovement();
    }

    private float AngleClamp(float angle) {
        while (angle < -179) {
            angle += 360;
        }
        while (angle > 181) {
            angle -= 360;
        }
        return angle;
    }

    private void HandleMovement() {
        if(Time.timeScale < float.Epsilon) return;

        // Read the user input
        int x = (Input.GetKey("e") ? 1 : 0) + (Input.GetKey("a") ? -1 : 0);
        int z = (Input.GetKey("q") ? 1 : 0) + (Input.GetKey("d") ? -1 : 0);
        int y = (Input.GetKey("w") ? 1 : 0) + (Input.GetKey("s") ? -1 : 0);
        int rotate = (Input.GetKey("left") ? 1 : 0) + (Input.GetKey("right") ? -1 : 0);

        if (x != 0 || y != 0 || z != 0) {
            transform.position += transform.rotation * new Vector3(x, y, z) * moveSpeed * Time.deltaTime;
        }
        // get current angle
        Vector3 m_TransformEulers = transform.localRotation.eulerAngles;
        float m_LookAngle = m_TransformEulers.y;
        m_LookAngle = AngleClamp(m_LookAngle);

        if (rotate  > .5 || rotate < -.5) {
            if (!rotateKeyDown) { // initial press
                m_AimingForIso += (rotate < 0) ? 90 : -90;
                m_AimingForIso = AngleClamp(m_AimingForIso);
                rotateKeyDown = true;
            }
        } else if (rotateKeyDown) {
            rotateKeyDown = false;
        }

        if (m_AimingForIso != m_LookAngle) {
            float direction = m_AimingForIso - m_LookAngle;
            direction = AngleClamp(direction);

            m_LookAngle += (direction > 0) ? m_HTurnSpeed : -m_HTurnSpeed;

            float newDirection = m_AimingForIso - m_LookAngle;
            newDirection = AngleClamp(newDirection);
            if (direction * newDirection <= 0) {
                if (x == 0) {
                    m_LookAngle = m_AimingForIso;
                } else {
                    m_AimingForIso += (x < 0) ? 90 : -90;
                    m_AimingForIso = AngleClamp(m_AimingForIso);
                }
            }
        }

        // Rotate the rig (the root object) around Y axis only:
        Quaternion m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

        if (m_TurnSmoothing > 0)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * Time.deltaTime);
        }
        else
        {
            transform.localRotation = m_TransformTargetRot;
        }
    }
}