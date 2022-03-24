using System;
using System.Collections;
using UnityEngine;

public class TreeMassPlacerOnPCG : TreeMassPlacer {
    [SerializeField] RunningOrderManager runningOrderManager;
    public event Action PostParentingEvent;

    [SerializeField] private PCGCubeMapGenerator _generator;
    public PCGCubeMapGenerator generator {
        get => _generator;
        set => _generator = value;
    }

    private void Awake() {
        repeatFor = 1;
        sizes.Add(new Vector3(_generator.mapChunkSize, 100, _generator.mapChunkSize));
        positions.Add(_generator.transform.position);
        offset = new Vector3((_generator.mapChunkSize-1)/-2f, 0, (_generator.mapChunkSize-1)/-2f);
        runningOrderManager = runningOrderManager ? runningOrderManager : RunningOrderManager.Instance;
        _generator.PostRiseEvent += TempReleaseAllFromMesh;
    }

    private void Start() {
        runningOrderManager.AddCallbackToCollection(3, Generate);
        runningOrderManager.AddCallbackToCollection(4, TempAddAllToMesh);
    }

    void TempAddAllToMesh() {
        StartCoroutine(DoTempAddAllToMesh());
    }

    IEnumerator DoTempAddAllToMesh() {
        while(!bReadyPrinted) yield return null;
        foreach(GameObject gO in _gOs) {
            gO.transform.SetParent(_generator.transform);
        }
        PostParentingEvent?.Invoke();
        yield return null;
    }

    void TempReleaseAllFromMesh() {
        foreach(GameObject gO in _gOs) {
            gO.transform.SetParent(null);
        }
    }
}
