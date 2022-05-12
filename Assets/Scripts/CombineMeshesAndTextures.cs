using UnityEngine;
using System.Collections;
 
public static class CombineMeshesAndTextures {
    public static TextureFormat textureFormat = TextureFormat.RGB24;

    private static Hashtable textureAtlas;
    private static Material combinedMaterial;
    private static int originalSize;
    private static int pow2;
    private static int size;

    public static void Combine(
        GameObject[] objectsToCombine,
        GameObject applyTo,
        bool useMipMaps = true
    ) {
        if(objectsToCombine == null || objectsToCombine.Length < 1) return;

        LoopOverRendererAndPreventNullTexture(objectsToCombine);

        PrepareValues(objectsToCombine);

        CreateCombinedTextureMaterial(objectsToCombine);
        UpdateUVToCombinedTexturePosition(objectsToCombine);
        UpdateAllMaterialToCombinedTextureMaterial(objectsToCombine);

        CombineMeshesAndApplyToTarget(objectsToCombine, applyTo);

        CleanUp(objectsToCombine);
    }

    private static void LoopOverRendererAndPreventNullTexture(GameObject[] objectsToCombine) {
        foreach(GameObject gO in objectsToCombine)
            if(!gO.GetComponent<Renderer>().sharedMaterial.mainTexture)
                gO.GetComponent<Renderer>().sharedMaterial.mainTexture = new Texture2D(256, 256);
    }

    private static void PrepareValues(GameObject[] objectsToCombine) {
        textureAtlas = new Hashtable();
        originalSize = objectsToCombine[0].GetComponent<Renderer>().sharedMaterial.mainTexture.width;
        pow2 = GetTextureSize(objectsToCombine);
        size =  pow2 * originalSize;
    }

    private static void CreateCombinedTextureMaterial(GameObject[] objectsToCombine, bool useMipMaps = true) {
        Texture2D combinedTexture = new Texture2D(size, size, textureFormat, useMipMaps);

        // Create the combined texture (remember to ensure the total size of the texture isn't
        // larger than the platform supports)
        for (int i = 0; i < objectsToCombine.Length; ++i) {
            Texture2D texture = (Texture2D)objectsToCombine[i].GetComponent<Renderer>().sharedMaterial.mainTexture;
            if (!textureAtlas.ContainsKey(texture)) {
                combinedTexture.SetPixels((i % pow2) * originalSize, (i / pow2) * originalSize, originalSize, originalSize, texture.GetPixels());
                textureAtlas.Add(texture, new Vector2(i % pow2, i / pow2));
            }
        }
        combinedTexture.Apply();

        combinedMaterial = new Material(objectsToCombine[0].GetComponent<Renderer>().sharedMaterial);
        combinedMaterial.mainTexture = combinedTexture;
    }

    private static void UpdateUVToCombinedTexturePosition(GameObject[] objectsToCombine) {
        Mesh mesh;

        // Update texture co-ords for each mesh (this will only work for meshes with coords betwen 0 and 1).
        for (int i = 0; i < objectsToCombine.Length; ++i) {            
            mesh = objectsToCombine[i].GetComponent<MeshFilter>().sharedMesh;
            Vector2[] uv = new Vector2[mesh.uv.Length];
            Vector2 offset;
            if (textureAtlas.ContainsKey(objectsToCombine[i].GetComponent<Renderer>().sharedMaterial.mainTexture)){
                offset = (Vector2)textureAtlas[objectsToCombine[i].GetComponent<Renderer>().sharedMaterial.mainTexture];
                for (int u = 0; u < mesh.uv.Length; ++u) {
                    uv[u] = mesh.uv[u] / (float)pow2;
                    uv[u].x += ((float)offset.x) / (float)pow2;
                    uv[u].y += ((float)offset.y) / (float)pow2;
                }
            } else {
                Debug.Log("Error: object must not be reused.");
            }
            
            mesh.uv = uv;
        }
    }

    private static void UpdateAllMaterialToCombinedTextureMaterial(GameObject[] objectsToCombine) {
        foreach(GameObject gO in objectsToCombine) {
            gO.GetComponent<Renderer>().material = combinedMaterial;
        }
    }

    private static void CombineMeshesAndApplyToTarget(GameObject[] objectsToCombine, GameObject applyTo) {
        // Combine given meshes
        CombineInstance[] combine = new CombineInstance[objectsToCombine.Length];
        for ( int i = 0; i < objectsToCombine.Length; i++) {
            combine[i].mesh = objectsToCombine[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = objectsToCombine[i].transform.localToWorldMatrix;
        }

        // Apply Merged Mesh to target
        MeshFilter filter = applyTo.GetComponent<MeshFilter>();
        MeshRenderer renderer = applyTo.GetComponent<MeshRenderer>();           
        filter.sharedMesh = new Mesh();
        filter.sharedMesh.CombineMeshes(combine);
        renderer.material = combinedMaterial;
    }

    private static void CleanUp(GameObject[] objectsToCombine) {
        // Make temp objects inactive
        for (int i = 0; i < objectsToCombine.Length; i++)
            objectsToCombine[i].SetActive(false);
        
        Resources.UnloadUnusedAssets();

        textureAtlas.Clear();
        textureAtlas = null;
        combinedMaterial = null;
        originalSize = 0;
        pow2 = 0;
        size = 0;
    }
   
    private static int GetTextureSize(GameObject[] o) {
        // Find unique textures
        ArrayList textures = new ArrayList();
        for (int i = 0; i < o.Length; i++) {
            if (!textures.Contains(o[i].GetComponent<Renderer>().sharedMaterial.mainTexture)) {
                textures.Add(o[i].GetComponent<Renderer>().sharedMaterial.mainTexture);
            }
        }

        if (textures.Count == 1) return 1;
        if (textures.Count <= 2*2) return 2;
        if (textures.Count <= 4*4) return 4;
        if (textures.Count <= 8*8) return 8;
        //extend if necessary.
        //eg. if(textures.Count <= 16*16) return 16;

        textures.Clear();
        return 0;
    }
}
 