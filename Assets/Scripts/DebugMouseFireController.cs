using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DebugMouseFireController : MonoBehaviour {
    InputManager inputManager;

    Camera mainCamera;
    [SerializeField] float raydistance = 200;
    [SerializeField] float fireSpreadRadius = 7f;
    [SerializeField] float extinguisherRadius = 5f;
    [SerializeField] GameObject TorchRangeIndicator, ExtinguishRangeIndicator;
    [SerializeField] Material TorchMaterial, ExtinguishMaterial;

    bool bShift = false;

    private void Awake() {
        mainCamera = mainCamera ? mainCamera : Camera.main;
    }

    private void Start() {
        inputManager = inputManager ? inputManager : InputManager.Instance;

        if(!TorchRangeIndicator) {
            TorchRangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            TorchRangeIndicator.name = "ArsonRangeIndicator";
            TorchRangeIndicator.transform.localScale = new Vector3(fireSpreadRadius, fireSpreadRadius, fireSpreadRadius) * 2;
            MeshRenderer renderer = TorchRangeIndicator.GetComponent<MeshRenderer>();
            renderer.material = TorchMaterial;
        }
        if(!ExtinguishRangeIndicator) {
            ExtinguishRangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ExtinguishRangeIndicator.name = "ExtinguishRangeIndicator";
            ExtinguishRangeIndicator.transform.localScale = new Vector3(extinguisherRadius, extinguisherRadius, extinguisherRadius) * 2;
            MeshRenderer renderer = ExtinguishRangeIndicator.GetComponent<MeshRenderer>();
            renderer.material = ExtinguishMaterial;
        }
        TorchRangeIndicator.SetActive(false);
        ExtinguishRangeIndicator.SetActive(false);

        inputManager.OnShift += Shift;
        inputManager.OnClick += Click;
    }

    void Update() {
        if(bShift) {
            if(ExtinguishRangeIndicator.activeSelf) 
                ExtinguishRangeIndicator.SetActive(false);
        } else {
            if(TorchRangeIndicator.activeSelf)
                TorchRangeIndicator.SetActive(false);
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, raydistance, 1 << LayerMask.NameToLayer("Land"))) {
            if(bShift) {
                TorchRangeIndicator.SetActive(true);
                TorchRangeIndicator.transform.position = hit.point;
            } else {
                ExtinguishRangeIndicator.SetActive(true);
                ExtinguishRangeIndicator.transform.position = hit.point;
            }
        } else {
            TorchRangeIndicator.SetActive(false);
            ExtinguishRangeIndicator.SetActive(false);
        }
    }

    void Shift(bool toggle) => bShift = toggle;

    void Click() {
        if(bShift) {
            RaycastHit[] hits = Physics.SphereCastAll(TorchRangeIndicator.transform.position, fireSpreadRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            foreach(RaycastHit sHit in hits) {
                if(sHit.transform.TryGetComponent<FireSpread>(out FireSpread spreadable))
                    spreadable.MarkTorched();
            }
        } else {
            RaycastHit[] hits = Physics.SphereCastAll(ExtinguishRangeIndicator.transform.position, extinguisherRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            foreach(RaycastHit sHit in hits) {
                if(sHit.transform.TryGetComponent<FireSpread>(out FireSpread spreadable))
                    spreadable.MarkExtinguished();
            }
        }
    }
}
