using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRandomSetFire : MonoBehaviour {
    [SerializeField] bool _bFire;
    public bool bFire {
        get => _bFire;
        set => _bFire = value;
    }

    [SerializeField] TreeMassPlacer placer;

    void Update() {
        if(_bFire) {
            _bFire = false;
            bool torched = false;
            Animator animator;
            if(placer.gOs[Random.Range(0, placer.gOs.Count-1)].TryGetComponent<Animator>(out animator)) {
                if(IsUnburnt(animator)) {
                    Arsonist.ArsonSingle(animator);
                    torched = true;
                }
            }
            if(animator != null && !torched) {
                foreach(var gO in placer.gOs) {
                    if(IsUnburnt(animator)) {
                        Arsonist.ArsonSingle(animator);
                        break;
                    }
                }
            }
        }
    }

    private bool IsUnburnt(Animator animator) {
        return animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Layer")).fullPathHash == Animator.StringToHash("Base Layer.Tree_Unburnt");
    }
}
