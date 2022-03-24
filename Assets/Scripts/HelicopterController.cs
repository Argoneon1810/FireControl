using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class HelicopterController : MonoBehaviour {
    [SerializeField] InputManager inputManager;

    [SerializeField] float movementSpeedMultiplier = 30;
    [SerializeField] float rotorSpeedMultiplier;
    [SerializeField] float rotationDuration;
    [SerializeField] Transform tailRotor, mainRotor;
    float turnSmoothVelocity;

    Rigidbody mRigidbody;
    CameraManager cameraManager;
    CameraManager.Direction cameraFacing;

    // Start is called before the first frame update
    void Start() {
        mRigidbody = GetComponent<Rigidbody>();
        cameraManager = cameraManager ? cameraManager : CameraManager.Instance;
        cameraFacing = cameraManager.currentCameraFacingDirection;
        cameraManager.OnFacingChanged += FacingChanged;
        inputManager = inputManager ? inputManager : InputManager.Instance;
        inputManager.OnMovement += Movement;
    }
    
    void FacingChanged(CameraManager.Direction d) {
        cameraFacing = d;
    }

    void Movement(float hori, float vert) {
        if(hori != 0 || vert != 0) {
            Vector3 force = Vector3.zero;
            force += cameraFacing.right * hori * movementSpeedMultiplier;
            force += cameraFacing.up * vert * movementSpeedMultiplier;
            mRigidbody.AddForce(force);
            float targetAngle = Mathf.Atan2(force.x, force.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle-90, ref turnSmoothVelocity, rotationDuration);
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }

    void Update() {
        if(mainRotor!=null)
            mainRotor.rotation = Quaternion.Euler(mainRotor.rotation.eulerAngles + Vector3.up * rotorSpeedMultiplier);
        if(tailRotor!=null)
            tailRotor.rotation = Quaternion.Euler(tailRotor.rotation.eulerAngles + Vector3.forward * rotorSpeedMultiplier);
    }
}
