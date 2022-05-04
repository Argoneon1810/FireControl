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

    private bool bSinkDone;

    private void Awake() {
        repeatFor = 1;
        sizes.Add(new Vector3(_generator.mapChunkSize, 100, _generator.mapChunkSize));
        positions.Add(_generator.transform.position);
        offset = new Vector3((_generator.mapChunkSize-1)/-2f, 0, (_generator.mapChunkSize-1)/-2f);
        runningOrderManager = runningOrderManager ? runningOrderManager : RunningOrderManager.Instance;
        _generator.PostSinkEvent += SinkDone;
        _generator.PostRiseEvent += DisattachAllFromLandmass;
    }

    public void Start() {
        runningOrderManager.AddCallbackToCollection(3, WaitForGenerateCall);
        runningOrderManager.AddCallbackToCollection(4, WaitAttachAllTreeToLandmass);
    }

    void SinkDone() {
        bSinkDone = true;
    }

    void WaitForGenerateCall() {
        StartCoroutine(GenerateCallWaiter());
    }

    IEnumerator GenerateCallWaiter() {
        while(!bSinkDone) {
            yield return null;
        }
        yield return new WaitForSeconds(.1f);   // TODO : Dunno why but when I come in to PCG scene from home scene, first few trees generates before sink is done
        Generate();
    }

    void WaitAttachAllTreeToLandmass() {
        StartCoroutine(TreeAttachCallWaiter());
    }

    IEnumerator TreeAttachCallWaiter() {
        while(!doneSpawn) yield return null;
        foreach(GameObject gO in _gOs) {
            gO.transform.SetParent(_generator.transform);
        }
        PostParentingEvent?.Invoke();
        yield return null;
    }

    void DisattachAllFromLandmass() {
        foreach(GameObject gO in _gOs) {
            gO.transform.SetParent(null);
        }
    }
}
