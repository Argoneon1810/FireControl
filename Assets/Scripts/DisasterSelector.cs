using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisasterSelector : MonoBehaviour {
    enum Disaster {
        Thunder,
        Meteorite
    }
    [SerializeField] Disaster disaster;
    [SerializeField] MeteoThrower thrower;
    [SerializeField] ThunderCloudSpawner spawner;
    [SerializeField] bool bDoNotChangeInspectorDisaster;

    private void Awake() {
        if(!bDoNotChangeInspectorDisaster) {
            Array values = Enum.GetValues(typeof(Disaster));
            disaster = (Disaster)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
        switch(disaster) {
            case Disaster.Thunder:
                thrower.doNotThrow = true;
                spawner.doNotSpawn = false;
                break;
            case Disaster.Meteorite:
                thrower.doNotThrow = false;
                spawner.doNotSpawn = true;
                break;
        }
    }
}
