using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderCloudSpawner : MonoBehaviour {
    [SerializeField] GameObject cloudPrefab;
    [SerializeField] CameraShaker cameraShaker;
    [SerializeField] private PCGCubeMapGenerator _generator;
    public PCGCubeMapGenerator generator {
        get => _generator;
        set => _generator = value;
    }
    [SerializeField] float cloudHeight;
    [SerializeField] float numOfCloudPerCall = 2;
    [SerializeField] bool debugSpawnThunderCloud;

    public bool doNotSpawn = true;

    void Start() {
        if(doNotSpawn) return;

        generator.PostRiseEvent += Spawn;
    }

    void Update() {
        if(debugSpawnThunderCloud) {
            debugSpawnThunderCloud = false;
            Spawn();
        }
    }

    void Spawn() {
        for(int i = 0; i < numOfCloudPerCall; ++i) {
            Vector3 point = _generator.GetPointOnSurface(Random.Range(0, _generator.mapChunkSize), Random.Range(0, _generator.mapChunkSize)) + Vector3.up * cloudHeight;
            ThunderCloud dropper = Instantiate(cloudPrefab, point, Quaternion.identity).GetComponent<ThunderCloud>();
            dropper.OnHit += new System.Action(cameraShaker.GetShake());
            dropper.Call();
        }
    }
}
