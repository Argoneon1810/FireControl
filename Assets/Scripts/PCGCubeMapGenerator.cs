using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System.IO;
// using UnityEngine.Networking;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshCollider))]
public class PCGCubeMapGenerator : MonoBehaviour {
    public class MeshData {
        public Vector3[] vertices;
        public int[] triangles;
        public List<Vector2[]> uvs = new List<Vector2[]>();

        int triangleIndex;

        public MeshData(int meshWidth, int meshHeight) {
            //for plane mesh
            //vertices = new Vector3[meshWidth * meshHeight];
            //uvs = new Vector2[meshWidth * meshHeight];
            //triangles = new int[(meshWidth-1)*(meshHeight-1)*6];

            vertices = new Vector3[meshWidth*meshHeight*2];
            triangles = new int[
                (meshWidth-1)*(meshHeight-1)*6*2 //top and bottom face
                + (meshWidth-1)*6*2           //sideface - width side; *(2-1) is skipped as it is unnecessary. its 2 because there are only two vertical vertices
                + (meshHeight-1)*6*2          //sideface - height side; *(2-1) is skipped as it is unnecessary. its 2 because there are only two vertical vertices
            ];
            AddNewUVChannel();
        }

        /// returns true if the mesh had less than 8 uv channels prior to current adding request
        /// returns false if there were 8 channels already added to the mesh
        public bool AddNewUVChannel() {
            if(uvs.Count >= 8) return false;

            uvs.Add(new Vector2[vertices.Length]);
            return true;
        }
        
        public void AddTriangle(int a, int b, int c) {
            triangles[triangleIndex] = a;
            triangles[triangleIndex+1] = b;
            triangles[triangleIndex+2] = c;
            triangleIndex += 3;
        }

        public Mesh CreateMesh() {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            for(int i = 0; i < uvs.Count; ++i) {
                switch(i) {
                    case 0:
                        mesh.uv  = uvs[i];
                        break;
                    case 1:
                        mesh.uv2 = uvs[i];
                        break;
                    case 2:
                        mesh.uv3 = uvs[i];
                        break;
                    case 3:
                        mesh.uv4 = uvs[i];
                        break;
                    case 4:
                        mesh.uv5 = uvs[i];
                        break;
                    case 5:
                        mesh.uv6 = uvs[i];
                        break;
                    case 6:
                        mesh.uv7 = uvs[i];
                        break;
                    case 7:
                        mesh.uv8 = uvs[i];
                        break;
                }
            }
            mesh.RecalculateNormals();
            return mesh;
        }
    }

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
    
    // const string TEXTIRESTOREDIR = "/SaveImages/";
    // const string TEXTUREFILENAME = "NoiseTextureImage";
    // const string TEXTUREEXTENSION = ".png";

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

        #region Noise Generation
        noiseMap = GenerateNoiseMap(_mapChunkSize, _seed, _noiseScale, _octaves, _persistance, _lacunarity, _offset);
        if(useFalloff) {
            float minVal = float.MaxValue;
            float[,] falloffMap = GenerateFalloffMap(_mapChunkSize, _falloffCurve);
            for(int i = 0; i < mapChunkSize; ++i) {
                for(int j = 0; j < mapChunkSize; ++j) {
                    if(_clampFalloff)
                        noiseMap[i, j] = Mathf.Clamp01(noiseMap[i, j] - falloffMap[i, j]);
                    else {
                        noiseMap[i, j] = noiseMap[i, j] - falloffMap[i, j];
                        if(minVal > noiseMap[i, j]) minVal = noiseMap[i, j];
                    }
                }
            }
            if(!_clampFalloff)
                for(int i = 0; i < mapChunkSize; ++i)
                    for(int j = 0; j < mapChunkSize; ++j)
                        noiseMap[i, j] -= minVal;
        }
        #endregion

        #region Mesh Generation
        Mesh mesh = GenerateTerrainChunk(noiseMap, _heightMultiplier, _heightGain).CreateMesh();
        // mesh.uv = ShrinkLeftUV(mesh.uv);
        //mesh.uv2 = ShrinkRightUV(mesh.uv2);
        //mesh.uv3 = ShrinkRightUV(mesh.uv3);
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        #endregion

        #region Texture Generation
        Texture2D mainTexture = CreateTextureByNoiseMap(noiseMap, _colorCurve, _bottomColor, _topColor) as Texture2D;
        // Texture2D subTexture = new Texture2D(_mapChunkSize, _mapChunkSize);
        // Color[] colorArray = subTexture.GetPixels();
        // for(int i = 0; i < colorArray.Length; ++i) {
        //     colorArray[i] = _bottomColor;
        // }
        // subTexture.SetPixels(colorArray);
        // subTexture.Apply();
        // Texture2D mergedTexture = MergeTexture(mainTexture, subTexture, _mapChunkSize, _mapChunkSize);
        // CreateTextureFileIfNecessary(texture);
        #endregion

        #region Apply Above to Components
        // StartCoroutine(OpenTextureFileAndApply(texture));
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", mainTexture);
        // GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", mergedTexture);
        // GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_SubTex", subTexture);
        GetComponent<MeshCollider>().sharedMesh = mesh;
        #endregion

        if(_matchTransformScaleToNoiseTexture)
            transform.localScale = new Vector3(_mapChunkSize, 1, _mapChunkSize);
    }

    private void ValidateValues() {
        if(_mapChunkSize <= 0) _mapChunkSize = 1;
        if(_lacunarity < 1) _lacunarity = 1;
        if(_octaves < 0) _octaves = 0;
        if(_octaves > 28) _octaves = 28;
    }

    #region Texture To File
    // Unnecessary!!!! Texture will be unloaded anyhow after exiting playmode. Saving it to file doesnt affect this.
    // private IEnumerator OpenTextureFileAndApply(Texture2D texture) {
    //     Again:
    //     UnityWebRequest www = UnityWebRequestTexture.GetTexture(Application.dataPath + TEXTIRESTOREDIR + TEXTUREFILENAME + TEXTUREEXTENSION);
    //     yield return www.SendWebRequest();
    //     try {
    //         texture = DownloadHandlerTexture.GetContent(www);
    //         if(GetComponent<Renderer>().sharedMaterial.mainTexture != texture)
    //             GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    //     } catch(InvalidOperationException e) {
    //         print(e.StackTrace);
    //         www.Dispose();
    //         CreateTextureFileIfNecessary(texture);
    //         print("again");
    //         goto Again;
    //     }
    // }

    // private void CreateTextureFileIfNecessary(Texture2D texture) {
    //     byte[] bytes = texture.EncodeToPNG();
        
    //     if(!Directory.Exists(Application.dataPath + TEXTIRESTOREDIR)) {
    //         Directory.CreateDirectory(Application.dataPath + TEXTIRESTOREDIR);
    //     }
    //     string filePath = Application.dataPath + TEXTIRESTOREDIR + TEXTUREFILENAME + TEXTUREEXTENSION;
    //     if(File.Exists(filePath)) {
    //         if(_changed) {
    //             _changed = false;
    //             print("overwritten");
    //             File.WriteAllBytes(filePath, bytes);    //Override
    //         }
    //     }
    //     else File.WriteAllBytes(filePath, bytes);       //File did not exist, so create
    // }
    #endregion

    public Vector3 GetPointOnSurface(float x, float z) {
        if(noiseMap != null)
            return new Vector3(
                x: x/-2f, 
                y: noiseMap[Mathf.FloorToInt(x), Mathf.FloorToInt(z)] * _heightMultiplier + _heightGain + baseHeightAdjustment,
                z: z/2f
            );
        else return Vector3.zero;
    }

    static float[,] GenerateNoiseMap(int mapLength, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[mapLength, mapLength];

        System.Random pRNG = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; ++i) {
            float offsetX = pRNG.Next(-100000, 100000) + offset.x;
            float offsetY = pRNG.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if(scale <= 0)
            scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfLength = mapLength / 2;

        for(int y = 0; y < mapLength; ++y) {
            for(int x = 0; x < mapLength; ++x) {
                float frequency, amplitude, noiseHeight;
                frequency = amplitude = noiseHeight = 1f;

                for(int a = 0; a < octaves; ++a) {
                    float tempX = (x - halfLength) / scale * frequency + octaveOffsets[a].x;
                    float tempY = (y - halfLength) / scale * frequency + octaveOffsets[a].y;
                    
                    float perlinValue = Mathf.PerlinNoise(tempX, tempY) /* *2-1 is to make perlinvalue bound to -1~1 scale */ * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if(noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                noiseMap[x,y] = noiseHeight;
            }
        }

        for(int y = 0; y < mapLength; ++y) {
            for(int x = 0; x < mapLength; ++x) {
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
            }
        }

        return noiseMap;
    }

    static float[,] GenerateFalloffMap(int size, AnimationCurve falloffCurve) {
        float[,] map = new float[size,size];
        for(int i = 0; i < size; ++i) {
            for(int j = 0; j < size; ++j) {
                float x = i / (float) size * 2 - 1;
                float y = j / (float) size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i,j] = falloffCurve.Evaluate(value);
            }
        }
        return map;
    }

    static Texture CreateTextureByNoiseMap(float[,] heightMap, AnimationCurve colorCurve, Color colorLow, Color colorHigh) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width*height];
        for(int y = 0; y < height; ++y) {
            for(int x = 0; x < width; ++x) {
                colorMap[y*width+x] = Color.LerpUnclamped(colorLow, colorHigh, colorCurve.Evaluate(heightMap[x, y]));
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    static MeshData GenerateTerrainChunk(float[,] heightMap, float multiplier, float gain) {
        int width      = heightMap.GetLength(0);
        int height     = heightMap.GetLength(1);
        float topLeftX = (width-1)/-2f;
        float topLeftZ = (height-1)/2f;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for(int z = 0; z < height; ++z) {
            for(int x = 0; x < width; ++x) {
                meshData.vertices[vertexIndex] = new Vector3(
                    x: topLeftX + x,
                    y: heightMap[x,z] * multiplier + gain,
                    z: topLeftZ - z)
                ;
                meshData.uvs[0][vertexIndex] = new Vector2((x/(float)width)/2, z/(float)height);

                if(x < width-1 && z < height-1) {
                    meshData.AddTriangle(vertexIndex, vertexIndex+1, vertexIndex+width);
                    meshData.AddTriangle(vertexIndex+width, vertexIndex+1, vertexIndex+width+1);
                }

                ++vertexIndex;
            }
        }

        if(meshData.AddNewUVChannel()) {
            for(int z = 0; z < height; ++z) {
                for(int x = 0; x < width; ++x) {
                    meshData.vertices[vertexIndex] = new Vector3(
                        x: topLeftX + x,
                        y: 0,
                        z: topLeftZ - z)
                    ;

                    meshData.uvs[0][vertexIndex] = new Vector2((x/(float)width)/2, z/(float)height);

                    if(x < width-1 && z < height-1) {
                        meshData.AddTriangle(vertexIndex,   vertexIndex+width, vertexIndex+1);
                        meshData.AddTriangle(vertexIndex+1, vertexIndex+width, vertexIndex+width+1);
                    }

                    ++vertexIndex;
                }
            }
        } else Debug.Log("Bottomface Generation: Something Went Wrong");

        if(meshData.AddNewUVChannel()) {
            for (int i = 0; i < width; ++i) {
                int bI = i+width*height;
                meshData.uvs[1][bI] = new Vector2(i/(float)width, 0);               //bottomface vertex  THERE IS BUG HERE
                meshData.uvs[1][i]  = new Vector2(i/(float)width, 1);               //topface vertex     THERE IS BUG HERE
                if(i < width-1) {
                    meshData.AddTriangle(bI,                    bI+1,                   i);
                    meshData.AddTriangle(i,                     bI+1,                   i+1);
                    meshData.AddTriangle(bI+(width*(height-1)), i+(width*(height-1))+1, bI+(width*(height-1))+1);
                    meshData.AddTriangle(i+(width*(height-1)),  i+(width*(height-1))+1, bI+(width*(height-1)));
                }
            }
        } else Debug.Log("Rimface Width Generation: Something Went Wrong");

        if(meshData.AddNewUVChannel()) {
            for (int i = 0; i < height; ++i) {
                int bZ = width*height;
                meshData.uvs[2][bZ + width*i] = new Vector2(0, i/(float)width);     //bottomface vertex  THERE IS BUG HERE
                meshData.uvs[2][width*i]      = new Vector2(1, i/(float)width);     //topface vertex     THERE IS BUG HERE
                if(i < height-1) {
                    meshData.AddTriangle(bZ+width*i,           width*i,                  bZ+width*(i+1));
                    meshData.AddTriangle(bZ+width*(i+1),       width*i,                  width*(i+1));
                    meshData.AddTriangle(bZ+(width-1)+width*i, bZ+(width-1)+width*(i+1), (width-1)+width*i);
                    meshData.AddTriangle((width-1)+width*i,    bZ+(width-1)+width*(i+1), (width-1)+width*(i+1));
                }
            }
        } else Debug.Log("Rimface Width Generation: Something Went Wrong");

        return meshData;
    }

    static Texture2D MergeTexture(Texture2D texA, Texture2D texB, int width, int height) {
        Color[] aArray = texA.GetPixels();
        Color[] bArray = texB.GetPixels();
        Color[] resultArray = new Color[aArray.Length];
        for(int y = 0; y < height; ++y) {
            for(int x = 0; x < width/2; ++x) {
                resultArray[x + y * height] = aArray[x*2 + y * height];
                resultArray[x + y * height + width/2] = bArray[x*2 + y * height];
            }
        }
        Texture2D toReturn = new Texture2D(width, height);
        toReturn.SetPixels(resultArray);
        toReturn.Apply();
        return toReturn;
    }

    static Vector2[] ShrinkLeftUV(Vector2[] uv) {
        Vector2[] resultUV = new Vector2[uv.Length];
        int counter = 0;
        foreach(Vector2 val in uv)
            resultUV[counter++] = new Vector2(val.x/2, val.y);
        return resultUV;
    }

    static Vector2[] ShrinkRightUV(Vector2[] uv) {
        Vector2[] resultUV = new Vector2[uv.Length];
        int counter = 0;
        foreach(Vector2 val in uv)
            resultUV[counter++] = new Vector2(val.x/2 + 0.5f, val.y);
        return resultUV;
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
