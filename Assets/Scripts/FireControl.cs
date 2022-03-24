using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FireControl : MonoBehaviour {
    InputManager inputManager;

    Camera mainCamera;
    [SerializeField] float raydistance = 200;
    [SerializeField] float fireSpreadRadius = 7f;
    [SerializeField] float extinguisherRadius = 5f;
    [SerializeField] GameObject ArsonRangeIndicator, ExtinguishRangeIndicator;
    [SerializeField] Material ArsonMaterial, ExtinguishMaterial;

    bool bShift = false;

    private void Awake() {
        mainCamera = mainCamera ? mainCamera : Camera.main;
    }

    private void Start() {
        inputManager = inputManager ? inputManager : InputManager.Instance;

        if(!ArsonRangeIndicator) {
            ArsonRangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ArsonRangeIndicator.name = "ArsonRangeIndicator";
            ArsonRangeIndicator.transform.localScale = new Vector3(fireSpreadRadius, fireSpreadRadius, fireSpreadRadius) * 2;
            var renderer = ArsonRangeIndicator.GetComponent<MeshRenderer>();
            renderer.material = ArsonMaterial;
        }
        if(!ExtinguishRangeIndicator) {
            ExtinguishRangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ExtinguishRangeIndicator.name = "ExtinguishRangeIndicator";
            ExtinguishRangeIndicator.transform.localScale = new Vector3(extinguisherRadius, extinguisherRadius, extinguisherRadius) * 2;
            var renderer = ExtinguishRangeIndicator.GetComponent<MeshRenderer>();
            renderer.material = ExtinguishMaterial;
        }
        ArsonRangeIndicator.SetActive(false);
        ExtinguishRangeIndicator.SetActive(false);

        inputManager.OnShift += Shift;
        inputManager.OnClick += Click;
    }

    void Update() {
        if(bShift) {
            if(ExtinguishRangeIndicator.activeSelf) 
                ExtinguishRangeIndicator.SetActive(false);
        } else {
            if(ArsonRangeIndicator.activeSelf)
                ArsonRangeIndicator.SetActive(false);
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, raydistance, 1 << LayerMask.NameToLayer("Land"))) {
            if(bShift) {
                ArsonRangeIndicator.SetActive(true);
                ArsonRangeIndicator.transform.position = hit.point;
            } else {
                ExtinguishRangeIndicator.SetActive(true);
                ExtinguishRangeIndicator.transform.position = hit.point;
            }
        } else {
            ArsonRangeIndicator.SetActive(false);
            ExtinguishRangeIndicator.SetActive(false);
        }
    }

    void Shift(bool toggle) => bShift = toggle;

    void Click() {
        List<Animator> animList = new List<Animator>();
        if(bShift) {
            RaycastHit[] hits = Physics.SphereCastAll(ArsonRangeIndicator.transform.position, fireSpreadRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            foreach(RaycastHit sHit in hits) {
                if(sHit.transform.TryGetComponent<Animator>(out Animator animator))
                    animList.Add(animator);
            }
            Arsonist.Arson(animList);
        } else {
            RaycastHit[] hits = Physics.SphereCastAll(ExtinguishRangeIndicator.transform.position, extinguisherRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            foreach(RaycastHit sHit in hits) {
                if(sHit.transform.TryGetComponent<Animator>(out Animator animator))
                    animList.Add(animator);
            }
            Extinguisher.Extinguish(animList);
        }
        animList.Clear();
    }
}
