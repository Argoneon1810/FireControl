using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterExtinguisher : MonoBehaviour {
    InputManager inputManager;

    GameObject ExtinguishRangeIndicator;

    [SerializeField] float extinguisherRadius = 5f;
    [SerializeField] float raydistance = 200;
    [SerializeField] Material ExtinguishMaterial;
    [SerializeField, Range(0,100)] int waterConsumption = 20;

    int _waterAmount = 100;
    public int waterAmount { get=>_waterAmount; }

    bool drainable = false;

    private void Start() {
        inputManager = inputManager ? inputManager : InputManager.Instance;
        inputManager.OnSpace += DoExtinguish;

        if(!ExtinguishRangeIndicator) {
            ExtinguishRangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ExtinguishRangeIndicator.name = "ExtinguishRangeIndicator Heli";
            ExtinguishRangeIndicator.transform.localScale = new Vector3(extinguisherRadius, extinguisherRadius, extinguisherRadius) * 2;
            var renderer = ExtinguishRangeIndicator.GetComponent<MeshRenderer>();
            renderer.material = ExtinguishMaterial;
        }
        ExtinguishRangeIndicator.SetActive(false);
    }

    void Update() {
        Ray ray = new Ray(transform.position, -transform.up);
        if(Physics.Raycast(ray, out RaycastHit hit, raydistance, 1 << LayerMask.NameToLayer("Land"))) {
            ExtinguishRangeIndicator.SetActive(true);
            ExtinguishRangeIndicator.transform.position = hit.point;
            drainable = false;
        } else {
            ExtinguishRangeIndicator.SetActive(false);
            drainable = true;
        }
    }

    void DoExtinguish() {
        if(drainable)
            _waterAmount = 100;
        else {
            if(_waterAmount <= 0) return;
            
            RaycastHit[] hits = Physics.SphereCastAll(ExtinguishRangeIndicator.transform.position, extinguisherRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            List<Animator> animList = new List<Animator>();
            foreach(RaycastHit sHit in hits) {
                if(sHit.transform.TryGetComponent<Animator>(out Animator animator))
                    animList.Add(animator);
            }
            Extinguisher.Extinguish(animList);
            _waterAmount -= waterConsumption;
        }
    }
}
