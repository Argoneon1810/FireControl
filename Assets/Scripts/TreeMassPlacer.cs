using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TreeMassPlacer : MonoBehaviour {
    [SerializeField] protected GameObject _treePrefab;
    [SerializeField] protected int _numOfTree;
    [SerializeField] protected int _maxTrialPerTree;
    [SerializeField] protected float _overlapCheckRadius;

    [SerializeField] protected bool _triggerRegen;
    [SerializeField] protected float _masterSpreadTime;
    [SerializeField] protected Vector3 _offset;

    protected EventfulCollection<GameObject> _gOs = new EventfulCollection<GameObject>();
    protected int numOfSpawned;
    protected int numOfFailed;

    protected int repeatFor;
    protected List<Vector3> sizes = new List<Vector3>();
    protected List<Vector3> positions = new List<Vector3>();

    protected float _spawnProgress;

    public bool doneSpawn;
    
    #region Getter Setter
    public GameObject treeToSpawn { get => _treePrefab; set => _treePrefab = value; }
    public int numberOfTree { get => _numOfTree; set => _numOfTree = value; }
    public int maxTrialPerTree { get => _maxTrialPerTree; set => _maxTrialPerTree = value; }
    public float radius { get => _overlapCheckRadius; set => _overlapCheckRadius = value; }
    public bool triggerRegen { get => _triggerRegen; set => _triggerRegen = value; }
    public EventfulCollection<GameObject> gOs { get => _gOs; }
    public Vector3 offset { get => _offset; set => _offset = value; }
    public float masterSpreadTime { get => _masterSpreadTime;
        set {
            _masterSpreadTime = value;
            if(_gOs != null) {
                foreach(GameObject gO in _gOs) {
                    FireSpread spreadable = gO.GetComponent<FireSpread>();
                    spreadable.spreadTimeGoal = value;
                }
            }
        }
    }
    public float spawnProgress { get => _spawnProgress; }
    public int GetTotalNumberOfTrees() => _gOs.Count;
    #endregion
    
    public void AddOnAddedEvent(EventHandler e) => _gOs.OnAdded += e;
    public void AddOnRemovedEvent(EventHandler e) => _gOs.OnRemoved += e;
    public void AddOnClearEvent(EventHandler e) => _gOs.OnClear += e;

    public void Generate() {
        for(int i = 0; i < repeatFor; ++i) {
            StartCoroutine(GenerateTrees(i));
        }
    }

    public virtual void Start() {
        StartCoroutine(CompletionCounter());
    }

    void Update() {
        if(_triggerRegen) {
            RegenerateTrees();
        }
    }

    void RegenerateTrees() {
        _triggerRegen = false;
        numOfSpawned = 0;
        numOfFailed = 0;
        ClearOldTrees();
        for(int i = 0; i < repeatFor; ++i) {
            StartCoroutine(GenerateTrees(i));
        }
    }
    
    void ClearOldTrees() {
        foreach(GameObject gO in _gOs) {
            Destroy(gO);
        }
        _gOs.Clear();
    }

    public IEnumerator GenerateTrees(int index) {
        yield return new WaitForEndOfFrame();       //slight delay needed as the first tree happens to be spawned before the landmesh sinks down, which make it float in the air
        
        Vector3 size = sizes[index];
        Vector3 position = positions[index];

        float height = size.y * 2 * 0.75f;
        for(int i = 0; i < _numOfTree; ++i) {
            for(int j = 0; j < _maxTrialPerTree; ++j) {
                Vector3 testPos = new Vector3(UnityEngine.Random.Range(position.x, position.x + size.x), height, UnityEngine.Random.Range(position.z, position.z + size.z)) + offset;
                if(!Physics.SphereCast(testPos + Vector3.up * 4 * 0.75f, _overlapCheckRadius, Vector3.down, out RaycastHit hit, height, 1 << LayerMask.NameToLayer("Flameable"))) {
                    GameObject gO = Instantiate(_treePrefab, testPos, Quaternion.Euler(-90,0,0));
                    gO.name = "Tree_"+i;

                    RaycastHit[] hits = Physics.RaycastAll(gO.transform.position, Vector3.down, height*2f, 1 << LayerMask.NameToLayer("Land"));
                    bool validSpawn = false;
                    foreach (RaycastHit hit2 in hits) {
                        gO.transform.position = hit2.point;
                        validSpawn = true;
                    }

                    if(validSpawn) {
                        ++numOfSpawned;
                        _gOs.Add(gO);
                        yield return null;
                    } else {
                        ++numOfFailed;
                        Destroy(gO);
                        yield return null;
                    }
                    break;
                }
            }
        }
        yield return null;
    }

    IEnumerator CompletionCounter() {
        int maxTreeCount = _numOfTree;
        while(numOfSpawned < maxTreeCount) {
            maxTreeCount = _numOfTree - numOfFailed;
            _spawnProgress = numOfSpawned / (float) maxTreeCount;
            yield return null;
        }
        doneSpawn = true;
    }
}