using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    public static InputManager Instance;
    public event Action OnTiltLeft, OnTiltRight, OnClick, OnSpace;
    public event Action<bool> OnShift;
    public event Action<float> OnScroll;
    public event Action<float, float> OnMovement;
    
    bool _tiltLeft, _tiltRight;
    public bool tiltLeft {
        get => _tiltLeft;
        set => _tiltLeft = value;
    }
    public bool tiltRight {
        get => _tiltRight;
        set => _tiltRight = value;
    }
    bool locked = true;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if(locked) return;

        if(Input.GetMouseButton(0)) OnClick?.Invoke();

        if(Input.GetKeyDown(KeyCode.Q) || _tiltLeft) {
            OnTiltLeft?.Invoke();
            _tiltLeft = false;
        }
        if(Input.GetKeyDown(KeyCode.E) || _tiltRight) {
            OnTiltRight?.Invoke();
            _tiltRight = false;
        }

        if(Input.GetKeyDown(KeyCode.Space)) OnSpace?.Invoke();

        OnShift?.Invoke(Input.GetKey(KeyCode.LeftShift));
        
        float hori = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        if(hori != 0 || vert != 0) OnMovement?.Invoke(hori, vert);

        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if(scroll != 0) OnScroll?.Invoke(scroll);
    }

    private void UnlockKeys() {
        locked = false;
    }
}
