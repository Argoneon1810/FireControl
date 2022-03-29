using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshCollider))]
public class PCGCubeMapGenerator : MonoBehaviour {

    #region Fields for noise generation
    [SerializeField] int _mapChunkSize;
    [SerializeField] Vector2 _offset;
    [SerializeField] float _noiseScale = 10f, _persistance = .5f, _lacunarity = 2;
    [SerializeField] int _octaves = 4, _seed;
    [SerializeField] bool _autoUpdate;
    [SerializeField] bool _matchTransformScaleToNoiseTexture;
    #endregion
    #region Fields for mesh generation
    [SerializeField] float _heightMultiplier = 1000f, _heightGain = 100f;
    [SerializeField] bool _useFalloff;
    [SerializeField] bool _clampFalloff;
    [SerializeField] AnimationCurve _falloffCurve;
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
    
    [SerializeField] bool _changed;

    [SerializeField] RunningOrderManager runningOrderManager;
    [SerializeField] TreeMassPlacerOnPCG _treeMassPlacerOnPCG;
    [SerializeField] InputManager inputManager;

    float[,] noiseMap;

    float baseHeightAdjustment;

    public event Action PostRiseEvent;

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
    public bool matchTransformScaleToNoiseTexture {
        get => _matchTransformScaleToNoiseTexture;
        set => _matchTransformScaleToNoiseTexture = value;
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
        get => _falloffCurve;
        set {
            if(_falloffCurve == null) _falloffCurve = AnimationCurve.Linear(0,0,1,1);
            _falloffCurve = value;
        }
    }
    public bool changed {
        get => _changed;
        set => _changed = value;
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
        baseHeightAdjustment = transform.position.y;
        inputManager = inputManager ? inputManager : InputManager.Instance;
        runningOrderManager = runningOrderManager ? runningOrderManager : RunningOrderManager.Instance;
        if(!_treeMassPlacerOnPCG) _treeMassPlacerOnPCG = GetComponent<TreeMassPlacerOnPCG>();
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
        ValidateValues();
        noiseMap = NoiseGenerator.Generate(_mapChunkSize, _seed, _noiseScale, _octaves, _persistance, _lacunarity, _offset, _useFalloff, _clampFalloff, _falloffCurve);

        Mesh mesh = MeshGenerator.GenerateTerrainChunk(noiseMap, _heightMultiplier, _heightGain).CreateMesh();
        mesh.RecalculateBounds();

        Texture2D mainTexture = TextureGenerator.CreateTextureByNoiseMap(noiseMap, _colorCurve, _bottomColor, _topColor) as Texture2D;

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", mainTexture);
        GetComponent<MeshCollider>().sharedMesh = mesh;

        if(_matchTransformScaleToNoiseTexture)
            transform.localScale = new Vector3(_mapChunkSize, 1, _mapChunkSize);
    }

    private void ValidateValues() {
        if(_mapChunkSize <= 0) _mapChunkSize = 1;
        if(_lacunarity < 1) _lacunarity = 1;
        if(_octaves < 0) _octaves = 0;
        if(_octaves > 28) _octaves = 28;
    }

    public Vector3 GetPointOnSurface(float x, float z) {
        if(noiseMap != null)
            return new Vector3(
                x: x - _mapChunkSize/2f,
                y: noiseMap[Mathf.FloorToInt(x), Mathf.FloorToInt(z)] * _heightMultiplier + _heightGain + baseHeightAdjustment,
                z: z - _mapChunkSize/2f
            );
        else return Vector3.zero;
    }
    void Sink() {
        transform.position += Vector3.down * sinkHeight;
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
