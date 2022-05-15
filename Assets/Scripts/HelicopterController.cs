using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class HelicopterController : MonoBehaviour {

    const int CONSTANT_ANGLE_DIFF = 90;

    public struct Direction {
        Vector3 _front, _back, _left, _right;
        public Direction(Vector3 up, Vector3 down, Vector3 left, Vector3 right) {
            _front = up;
            _back = down;
            _left = left;
            _right = right;
        }
        public Vector3 up { get => _front; }
        public Vector3 down { get => _back; }
        public Vector3 left { get => _left; }
        public Vector3 right { get => _right; }

        public bool Equals(Direction d) {
            if(d.up != _front) return false;
            if(d.down != _back) return false;
            if(d.left != _left) return false;
            if(d.right != _right) return false;
            return true;
        }
    }
    public struct Directions {
        Direction _N, _S, _E, _W;
        public Directions(Direction N, Direction S, Direction E, Direction W) {
            _N = N;
            _S = S;
            _E = E;
            _W = W;
        }
        public Direction N { get => _N; }
        public Direction S { get => _S; }
        public Direction E { get => _E; }
        public Direction W { get => _W; }

        public Direction GetLeftOf(Direction d) {
            if(d.Equals(N)) return W;
            else if(d.Equals(W)) return S;
            else if(d.Equals(S)) return E;
            else return N;
        }
        public Direction GetRightOf(Direction d) {
            if(d.Equals(N)) return E;
            else if(d.Equals(E)) return S;
            else if(d.Equals(S)) return W;
            else return N;
        }
    }

    [SerializeField] InputManager inputManager;

    [SerializeField] float movementSpeedMultiplier = 30;
    [SerializeField] float rotationDuration;
    float turnSmoothVelocity;

    Rigidbody mRigidbody;
    CameraManager cameraManager;
    Direction cameraFacing;

    // Start is called before the first frame update
    void Start() {
        mRigidbody = GetComponent<Rigidbody>();
        cameraManager = cameraManager ? cameraManager : CameraManager.Instance;
        cameraFacing = cameraManager.currentCameraFacingDirection;
        cameraManager.OnFacingChanged += FacingChanged;
        inputManager = inputManager ? inputManager : InputManager.Instance;
        inputManager.OnMovement += Movement;
    }
    
    void FacingChanged(Direction d) {
        cameraFacing = d;
    }

    void Movement(float hori, float vert) {
        if(hori != 0 || vert != 0) {
            Vector3 force = Vector3.zero;
            force += cameraFacing.right * hori * movementSpeedMultiplier;
            force += cameraFacing.up * vert * movementSpeedMultiplier;
            mRigidbody.AddForce(force);
            float targetAngle = Mathf.Atan2(force.x, force.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle - CONSTANT_ANGLE_DIFF,
                ref turnSmoothVelocity,
                rotationDuration
            );
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
}
