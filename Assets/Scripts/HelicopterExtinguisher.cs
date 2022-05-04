using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HelicopterExtinguisher : MonoBehaviour {
    InputManager inputManager;

    GameObject ExtinguishRangeIndicator;

    // [SerializeField] Material _extinguisherMaterial;
    public Material _extinguisherMaterial;
    float _extinguisherRadius = 5f;
    float _groundCheckRayMaxDistance = 200;
    int _waterConsumptionPerSpray = 20;
    int _totalWaterAmount = 100;

    public float extinguisherRadius {
        get => _extinguisherRadius;
        set => _extinguisherRadius = value;
    }
    public float raydistance {
        get => _groundCheckRayMaxDistance;
        set => _groundCheckRayMaxDistance = value;
    }
    // public Material extinguisherMaterial {
    //     get => _extinguisherMaterial;
    //     set => _extinguisherMaterial = value;
    // }
    public int waterConsumptionPerSpray {
        get => _waterConsumptionPerSpray;
        set => _waterConsumptionPerSpray = value;
    }
    public int totalWaterAmount { get=>_totalWaterAmount; }

    bool drainable = false;

    private void Start() {
        inputManager = inputManager ? inputManager : InputManager.Instance;
        inputManager.OnSpace += DoExtinguish;

        // TODO : Due to the problem where Material is unserializable, Custom Editor could not load Extinguisher Material
        //        Therefore, I loaded material by script. If further solution is found, do change.
        _extinguisherMaterial = EditorGUIUtility.Load("Assets/Materials/ExtinguishMaterial.mat") as Material;

        InitializeValues();
    }

    void Update() {
        Ray ray = new Ray(transform.position, -transform.up);
        if(Physics.Raycast(ray, out RaycastHit hit, _groundCheckRayMaxDistance, 1 << LayerMask.NameToLayer("Land"))) {
            ExtinguishRangeIndicator.SetActive(true);
            ExtinguishRangeIndicator.transform.position = hit.point;
            drainable = false;
        } else {
            ExtinguishRangeIndicator.SetActive(false);
            drainable = true;
        }
    }

    public void ChangeRadius() {
        ExtinguishRangeIndicator.transform.localScale = new Vector3(_extinguisherRadius, _extinguisherRadius, _extinguisherRadius) * 2;
    }

    void InitializeValues() {
        if(!ExtinguishRangeIndicator) {
            ExtinguishRangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ExtinguishRangeIndicator.name = "ExtinguishRangeIndicator Heli";
            ExtinguishRangeIndicator.layer = LayerMask.NameToLayer("Extinguisher");
            ExtinguishRangeIndicator.transform.localScale = new Vector3(_extinguisherRadius, _extinguisherRadius, _extinguisherRadius) * 2;
            MeshRenderer renderer = ExtinguishRangeIndicator.GetComponent<MeshRenderer>();
            renderer.material = _extinguisherMaterial;
        }
        ExtinguishRangeIndicator.SetActive(false);
    }

    void DoExtinguish() {
        if(drainable)
            _totalWaterAmount = 100;
        else {
            if(_totalWaterAmount <= 0) return;
            
            RaycastHit[] hits = Physics.SphereCastAll(ExtinguishRangeIndicator.transform.position, _extinguisherRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            foreach(RaycastHit sHit in hits) {
                if(sHit.transform.TryGetComponent<FireSpread>(out FireSpread spreadable))
                    spreadable.MarkExtinguished();
            }
            _totalWaterAmount -= _waterConsumptionPerSpray;
        }
    }
}
