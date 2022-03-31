// using System;
// using System.Threading;
// using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class MaterialBlender : MonoBehaviour {
    [SerializeField] private Material[] _fromMaterials;
    [SerializeField] private Material[] _toMaterials;
    [SerializeField] private float _transitionLength;
    [SerializeField] private Renderer _mRenderer;
    [SerializeField] private bool _reverse = false;

    public float transitionLength {
        get => _transitionLength;
        set => _transitionLength = value;
    }
    public Renderer mRenderer {
        get => _mRenderer;
        set => _mRenderer = value;
    }
    public bool reverse {
        get => _reverse;
        set => _reverse = value;
    }

    public Material GetFromMaterialAt(int index) => _fromMaterials[index];
    public Material GetToMaterialAt(int index) => _toMaterials[index];
    
    #region Coroutine Version
    public void TriggerBlend() {
        if (_fromMaterials == null || _toMaterials == null || _fromMaterials.Length == 0 || _toMaterials.Length == 0)
            print("Either of the list of materials are empty");

        else if(_fromMaterials.Length > _toMaterials.Length || _fromMaterials.Length < _toMaterials.Length)
            print("Size mismatch between two list of materials");

        else if(_fromMaterials.Length > _mRenderer.materials.Length || _fromMaterials.Length < _mRenderer.materials.Length)
            print("Size mismatch between original materials and transitioning materials");

        else
            for(int i = 0; i < _fromMaterials.Length; ++i)
                StartCoroutine(BlendMaterial(i));
    }

    IEnumerator BlendMaterial(int index) {
        float timePassed = 0;
        while(timePassed < _transitionLength) {
            float transitionValue = (((/*Current Tick*/ timePassed - /*Starting Tick*/ 0) * (/*Target Max*/ 1 - /*Target Floor*/ 0)) / (/*Original Max*/ _transitionLength - /*Starting Tick*/ 0)) + /*Target Floor*/ 0;
            if(reverse)
                _mRenderer.materials[index].Lerp(
                    _toMaterials[index],
                    _fromMaterials[index],
                    transitionValue
                );
            else
                _mRenderer.materials[index].Lerp(
                    _fromMaterials[index],
                    _toMaterials[index],
                    transitionValue
                );
            timePassed += Time.deltaTime;
            yield return null;
        }
    }
    #endregion
}
