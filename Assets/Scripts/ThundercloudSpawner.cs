using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThundercloudSpawner : MonoBehaviour {
    [SerializeField] GameObject cloudPrefab;
    [SerializeField] CameraShaker cameraShaker;
    [SerializeField] private PCGCubeMapGenerator _generator;
    public PCGCubeMapGenerator generator {
        get => _generator;
        set => _generator = value;
    }
    [SerializeField] float cloudHeight;

    public bool doNotSpawn = true;

    void Start() {
        if(doNotSpawn) return;

        generator.PostRiseEvent += Spawn;
    }

    void Spawn() {
        Vector3 point = _generator.GetPointOnSurface(Random.Range(0, _generator.mapChunkSize), Random.Range(0, _generator.mapChunkSize)) + Vector3.up * cloudHeight;
        GameObject cloud = Instantiate(cloudPrefab, point, Quaternion.identity);
        var dropper = cloud.GetComponent<ThunderDropper>();
        dropper.OnHit += new System.Action(cameraShaker.GetShake());
        dropper.Call();
    }
}
