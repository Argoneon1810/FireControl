using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoThrower : MonoBehaviour {
    [SerializeField] GameObject meteoPrefab;
    [SerializeField] CameraShaker cameraShaker;
    [SerializeField] private PCGCubeMapGenerator _generator;
    public PCGCubeMapGenerator generator {
        get => _generator;
        set => _generator = value;
    }

    [SerializeField] bool debugShootMeteorite;

    [SerializeField] int repeatFor = 1;
    Vector3 size, position;

    [SerializeField] float meteoLaunchHeight = 40f;

    public bool doNotThrow = true;

    private void Awake() {
        size = new Vector3(_generator.mapChunkSize, 100, _generator.mapChunkSize);
        position = _generator.transform.position;
    }

    void Start() {
        if(doNotThrow) return;

        generator.PostRiseEvent += Shot;
    }

    void Update() {
        if(debugShootMeteorite) {
            debugShootMeteorite = false;
            Shot();
        }
    }

    void Shot() {
        StartCoroutine(ShootMeteo());
    }

    IEnumerator ShootMeteo() {
        for(int i = 0; i < repeatFor; ++i) {
            //pick any point on a surface
            Vector3 point = _generator.GetPointOnSurface(Random.Range(0, _generator.mapChunkSize), Random.Range(0, _generator.mapChunkSize));
            //random angle to enter targeted point
            Vector3 randDir = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f)).normalized;
            //put meteorite to the position
            GameObject meteo = Instantiate(meteoPrefab, point + randDir * meteoLaunchHeight, Quaternion.Euler(Random.Range(0,360), Random.Range(0,360), Random.Range(0,360)));
            //assign travel info
            MeteoBody body = meteo.GetComponent<MeteoBody>();
            body.SetGravity(-randDir);
            body.OnHit += new System.Action(cameraShaker.GetShake());
            
            yield return null;
        }
    }
}
