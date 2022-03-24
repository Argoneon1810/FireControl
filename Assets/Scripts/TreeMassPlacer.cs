using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public abstract class TreeMassPlacer : MonoBehaviour {
    [SerializeField] protected GameObject _treeToSpawn;
    [SerializeField] protected int _numberOfTree;
    [SerializeField] protected int _maxTrialPerTree;
    [SerializeField] protected float _radius;
    [SerializeField] protected bool _triggerRegen;
    [SerializeField] protected float _masterSpreadTime;
    [SerializeField] protected Vector3 _offset;

    protected EventfulCollection<GameObject> _gOs = new EventfulCollection<GameObject>();
    protected bool bReadyPrinted;
    protected int completionCounter;
    protected int numOfBurnt;

    protected int repeatFor;
    protected List<Vector3> sizes = new List<Vector3>();
    protected List<Vector3> positions = new List<Vector3>();

    public event Action OnTrialSpawn;
    
    #region Getter Setter
    public GameObject treeToSpawn { get => _treeToSpawn; set => _treeToSpawn = value; }
    public int numberOfTree { get => _numberOfTree; set => _numberOfTree = value; }
    public int maxTrialPerTree { get => _maxTrialPerTree; set => _maxTrialPerTree = value; }
    public float radius { get => _radius; set => _radius = value; }
    public bool triggerRegen { get => _triggerRegen; set => _triggerRegen = value; }
    public EventfulCollection<GameObject> gOs { get => _gOs; }
    public Vector3 offset { get => _offset; set => _offset = value; }
    public float masterSpreadTime { get => _masterSpreadTime;
        set {
            _masterSpreadTime = value;
            if(_gOs != null) {
                foreach(var gO in _gOs) {
                    Animator animator = gO.GetComponent<Animator>();
                    AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
                    foreach (var state in controller.layers[animator.GetLayerIndex("Base Layer")].stateMachine.states) {
                        if(state.state.name.Equals("Tree_OnFire")) {
                            foreach(var transition in state.state.transitions) {
                                if(transition.name.Equals("Manipulate This")) {
                                    transition.exitTime = value;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public int GetTotalNumberOfTrees() => _gOs.Count;
    public float GetGreenAmount() => 1 - GetBurntAmount();
    public float GetBurntAmount() => numOfBurnt / (float)_gOs.Count;
    public bool IsDone() => bReadyPrinted;
    #endregion
    
    public void AddOnAddedEvent(EventHandler e) => _gOs.OnAdded += e;
    public void AddOnRemovedEvent(EventHandler e) => _gOs.OnRemoved += e;
    public void AddOnClearEvent(EventHandler e) => _gOs.OnClear += e;

    public void Generate() {
        for(int i = 0; i < repeatFor; ++i) {
            StartCoroutine(GenerateTrees(i));
        }
    }

    void Update() {
        if(_triggerRegen) {
            _triggerRegen = false;
            completionCounter = 0;
            ClearTrees();
            for(int i = 0; i < repeatFor; ++i) {
                StartCoroutine(GenerateTrees(i));
            }
        }
        if(completionCounter == repeatFor && !bReadyPrinted) {
            bReadyPrinted = true;
        }
    }
    
    void ClearTrees() {
        foreach(GameObject gO in _gOs) {
            Destroy(gO);
        }
        _gOs.Clear();
    }

    virtual public IEnumerator GenerateTrees(int index) {
        yield return new WaitForEndOfFrame();       //slight delay needed as the first tree happens to be spawned before the landmesh sinks down, which make it float in the air
        
        Vector3 size = sizes[index];
        Vector3 position = positions[index];

        float height = size.y * 2 * 0.75f;
        for(int i = 0; i < _numberOfTree; ++i) {
            OnTrialSpawn?.Invoke();
            for(int j = 0; j < _maxTrialPerTree; ++j) {
                Vector3 testPos = new Vector3(UnityEngine.Random.Range(position.x, position.x + size.x), height, UnityEngine.Random.Range(position.z, position.z + size.z)) + offset;
                if(!Physics.SphereCast(testPos + Vector3.up * 4 * 0.75f, _radius, Vector3.down, out RaycastHit hit, height, 1 << LayerMask.NameToLayer("Flameable"))) {
                    GameObject gO = Instantiate(_treeToSpawn, testPos, Quaternion.Euler(-90,0,0));
                    gO.name = "Tree_"+i;

                    RaycastHit[] hits = Physics.RaycastAll(gO.transform.position, Vector3.down, height*2f, 1 << LayerMask.NameToLayer("Land"));
                    bool validSpawn = false;
                    foreach (var hit2 in hits) {
                        gO.transform.position = hit2.point;
                        validSpawn = true;
                    }

                    if(validSpawn) {
                        _gOs.Add(gO);
                        yield return null;
                    } else {
                        Destroy(gO);
                        yield return null;
                    }
                    break;
                }
            }
        }
        yield return null;

        ++completionCounter;
    }

    public void Burnt() => ++numOfBurnt;
}