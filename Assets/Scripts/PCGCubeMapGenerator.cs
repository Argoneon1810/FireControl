using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshCollider))]
public class PCGCubeMapGenerator : MonoBehaviour {
    public event Action PostSinkEvent, PostRiseEvent;

    #region Fields for noise generation
    [SerializeField] int _mapChunkSize;
    [SerializeField] Vector2 _offset;
    [SerializeField] float _noiseScale = 10f, _persistance = .5f, _lacunarity = 2;
    [SerializeField] int _octaves = 4, _seed;
    [SerializeField] bool _autoUpdate;
    float[,] noiseMapUsedForLastestGeneration;
    #endregion
    #region Fields for mesh generation
    [SerializeField] float _heightMultiplier = 1000f, _heightGain = 100f;
    [SerializeField] bool _useFalloff;
    [SerializeField] bool _clampFalloff;
    [SerializeField] AnimationCurve _edgeFalloffCurve = AnimationCurve.EaseInOut(0,0,1,1);
    float heightAdjustmentMadeInEditorMode;
    #endregion
    #region Fields for texture generation
    [SerializeField] AnimationCurve _colorCurve;
    [SerializeField] Color _topColor, _bottomColor;
    #endregion
    #region Fields for Animating Scriptually
    [SerializeField] float _sinkHeight;
    [SerializeField] float _riseDuration;
    [SerializeField] AnimationCurve _riseCurve;
    #endregion

    InputManager inputManager;
    RunningOrderManager runningOrderManager;
    [SerializeField] TreeMassPlacerOnPCG _treeMassPlacerOnPCG;

    #region Getter Setter
    public int mapChunkSize {
        get => _mapChunkSize;
        set => _mapChunkSize = value;
    }
    public Vector2 offset {
        get => _offset;
        set => _offset = value;
    }
    public float noiseScale {
        get => _noiseScale;
        set => _noiseScale = value;
    }
    public float persistance {
        get => _persistance;
        set => _persistance = value;
    }
    public float lacunarity {
        get => _lacunarity;
        set => _lacunarity = value;
    }
    public float heightMultiplier {
        get => _heightMultiplier;
        set => _heightMultiplier = value;
    }
    public float heightGain {
        get => _heightGain;
        set => _heightGain = value;
    }
    public int octaves {
        get => _octaves;
        set => _octaves = value;
    }
    public int seed {
        get => _seed;
        set => _seed = value;
    }
    public bool autoUpdate {
        get => _autoUpdate;
        set => _autoUpdate = value;
    }
    public bool useFalloff {
        get => _useFalloff;
        set => _useFalloff = value;
    }
    public bool clampFalloff {
        get => _clampFalloff;
        set => _clampFalloff = value;
    }
    public AnimationCurve falloffCurve {
        get => _edgeFalloffCurve;
        set {
            if(_edgeFalloffCurve == null) _edgeFalloffCurve = AnimationCurve.Linear(0,0,1,1);
            _edgeFalloffCurve = value;
        }
    }
    public AnimationCurve colorCurve {
        get => _colorCurve;
        set {
            if(_colorCurve == null) _colorCurve = AnimationCurve.Linear(0,0,1,1);
            _colorCurve = value;
        }
    }
    public Color topColor {
        get => _topColor;
        set => _topColor = value;
    }
    public Color bottomColor {
        get => _bottomColor;
        set => _bottomColor = value;
    }
    public float sinkHeight {
        get => _sinkHeight;
        set => _sinkHeight = value;
    }
    public float riseDuration {
        get => _riseDuration;
        set => _riseDuration = value;
    }
    public AnimationCurve riseCurve {
        get => _riseCurve;
        set {
            if(_riseCurve == null) _riseCurve = AnimationCurve.Linear(0,0,1,1);
            _riseCurve = value;
        }
    }
    public TreeMassPlacerOnPCG treeMassPlacerOnPCG {
        get => _treeMassPlacerOnPCG;
        set => _treeMassPlacerOnPCG = value;
    }
    #endregion

    private void Awake() {
        runningOrderManager = runningOrderManager ? runningOrderManager : RunningOrderManager.Instance;
        inputManager = inputManager ? inputManager : InputManager.Instance;

        //this should be always false unless inspector value is broken
        if(_treeMassPlacerOnPCG == null) _treeMassPlacerOnPCG = GetComponent<TreeMassPlacerOnPCG>();

        heightAdjustmentMadeInEditorMode = transform.position.y;

        _treeMassPlacerOnPCG.PostParentingEvent += Rise;
    }

    private void Start() {
        _riseCurve = _riseCurve != null ? _riseCurve : AnimationCurve.EaseInOut(0,0,1,1);
        runningOrderManager.AddCallbackToCollection(0, RandomizeSeed);
        runningOrderManager.AddCallbackToCollection(1, Generate);
        runningOrderManager.AddCallbackToCollection(2, Sink);
    }

    public void RandomizeSeed() {
        _seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        _offset = new Vector2(UnityEngine.Random.Range(-1000, 1000), UnityEngine.Random.Range(-1000, 1000));
    }

    public void Generate() {
        ValidateInitialValues();
        
        float[,] noiseMap = CreateNoiseForSurfaceGeneration();

        MeshGenerator.MeshData[] meshes = MeshGenerator.GenerateTerrainChunk(
            noiseMap,
            _heightMultiplier,
            _heightGain
        );
        Texture2D topSurfaceTexture = TextureGenerator.CreateTextureByNoiseMap(
            noiseMap,
            _colorCurve,
            _bottomColor,
            _topColor
        );
        Texture2D otherSurfaceTexture = TextureGenerator.CreateTextureMonoColor(
            noiseMap.GetLength(0),
            noiseMap.GetLength(1),
            _bottomColor
        );

        GameObject[] tempPCGMeshGameObjects = ConvertPCGMeshesIntoGameObjects(meshes);
        ApplyTextureToTemporaryGameObjects(tempPCGMeshGameObjects, topSurfaceTexture, otherSurfaceTexture);
        MergePCGMeshesToSelf(tempPCGMeshGameObjects);
        DeleteTemporaryMeshes(tempPCGMeshGameObjects);

        noiseMapUsedForLastestGeneration = noiseMap;
    }

    private float[,] CreateNoiseForSurfaceGeneration() {
        NoiseGenerator.Noise mPerlinNoise = NoiseGenerator.CreatePerlinNoise(
            _mapChunkSize,
            _seed, 
            _noiseScale, 
            _octaves, 
            _persistance, 
            _lacunarity, 
            _offset
        );
        if(_useFalloff) {
            if(_clampFalloff) {
                mPerlinNoise.ApplySecondNoise(
                    NoiseGenerator.GenerateFalloff(
                        _mapChunkSize,
                        _edgeFalloffCurve
                    )
                );
            } else {
                mPerlinNoise.ApplySecondNoiseUnclamped(
                    NoiseGenerator.GenerateFalloff(
                        _mapChunkSize,
                        _edgeFalloffCurve
                    )
                );
            }
        }
        return mPerlinNoise.Generate();
    }

    private void ValidateInitialValues() {
        if(_mapChunkSize <= 0) _mapChunkSize = 1;
        if(_lacunarity < 1) _lacunarity = 1;
        if(_octaves < 0) _octaves = 0;
        if(_octaves > 28) _octaves = 28;
    }

    private GameObject[] ConvertPCGMeshesIntoGameObjects(MeshGenerator.MeshData[] meshes) {
        GameObject[] toReturnGameObjects = new GameObject[meshes.Length];
        for(int i = 0; i < toReturnGameObjects.Length; ++i) {
            toReturnGameObjects[i] = new GameObject();
            toReturnGameObjects[i].AddComponent<MeshFilter>().mesh = meshes[i].CreateMesh();
            toReturnGameObjects[i].AddComponent<MeshRenderer>().sharedMaterial = Instantiate(GetComponent<MeshRenderer>().sharedMaterial);
            toReturnGameObjects[i].tag = "TemporaryMesh";   //just in case
        }
        return toReturnGameObjects;
    }

    private void ApplyTextureToTemporaryGameObjects(GameObject[] tempPCGMeshGameObjects, Texture2D topSurfaceTexture, Texture2D otherSurfaceTexture) {
        for(int i = 0; i < tempPCGMeshGameObjects.Length; ++i) {
            if(i == MeshGenerator.TOP_INDEX)
                tempPCGMeshGameObjects[i].GetComponent<MeshRenderer>().sharedMaterial.mainTexture = topSurfaceTexture;
            else 
                tempPCGMeshGameObjects[i].GetComponent<MeshRenderer>().sharedMaterial.mainTexture = otherSurfaceTexture;
        }
    }

    private void MergePCGMeshesToSelf(GameObject[] tempPCGMeshGameObjects) {
        CombineMeshesAndTextures.Combine(tempPCGMeshGameObjects, gameObject);
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }

    private void DeleteTemporaryMeshes(GameObject[] tempPCGMeshGameObjects) {
        foreach(GameObject gO in tempPCGMeshGameObjects) {
            DestroyImmediate(gO);
        }
    }

    public Vector3 GetPointOnSurface(float x, float z) {
        if(noiseMapUsedForLastestGeneration != null)
            return new Vector3(
                x: x - _mapChunkSize/2f,
                y: noiseMapUsedForLastestGeneration[Mathf.FloorToInt(x), Mathf.FloorToInt(z)]
                    * _heightMultiplier + _heightGain + heightAdjustmentMadeInEditorMode,
                z: z - _mapChunkSize/2f
            );
        else return Vector3.zero;
    }
    void Sink() {
        transform.position += Vector3.down * sinkHeight;
        PostSinkEvent?.Invoke();
    }

    void Rise() {
        StartCoroutine(DoRise());
    }

    IEnumerator DoRise() {
        float t = 0;
        float origHeigh = transform.position.y;
        while(t < _riseDuration) {
            t+=Time.deltaTime;
            transform.position = new Vector3(
                x: transform.position.x,
                y: Mathf.Lerp(origHeigh, origHeigh+_sinkHeight, _riseCurve.Evaluate(t/_riseDuration)),
                z: transform.position.z
            );
            yield return null;
        }
        PostRiseEvent?.Invoke();
        inputManager.SendMessage("UnlockKeys");
        yield return null;
    }
}
