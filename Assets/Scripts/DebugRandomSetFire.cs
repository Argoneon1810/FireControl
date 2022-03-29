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
            if(placer.gOs[Random.Range(0, placer.gOs.Count-1)].TryGetComponent<FireSpread>(out FireSpread spreadable)) {
                if(IsUnburnt(spreadable)) {
                    spreadable.MarkTorched();
                    torched = true;
                }
            }
            if(spreadable != null && !torched) {
                foreach(GameObject gO in placer.gOs) {
                    if(IsUnburnt(spreadable)) {
                        spreadable.MarkTorched();
                        break;
                    }
                }
            }
        }
    }

    private bool IsUnburnt(FireSpread spreadable) {
        return !spreadable.bIsBurnt && !spreadable.bIsOnFire;
    }
}
