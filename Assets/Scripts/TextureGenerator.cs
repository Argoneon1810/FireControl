using UnityEngine;

public static class TextureGenerator {
    public static Texture2D CreateTextureByNoiseMap(float[,] heightMap, AnimationCurve colorCurve, Color colorLow, Color colorHigh) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[texture.width * texture.height];
        for(int y = 0; y < height; ++y) {
            for(int x = 0; x < width; ++x) {
                colorMap[y*width+x] = Color.LerpUnclamped(colorLow, colorHigh, colorCurve.Evaluate(heightMap[x, y]));
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    public static Texture2D CreateTextureMonoColor(int width, int height, Color color) {
        Texture2D texture = new Texture2D(width, height);
        Color[] colorMap = new Color[texture.width * texture.height];

        for(int y = 0; y < height; ++y)
            for(int x = 0; x < width; ++x)
                colorMap[y*width+x] = color;

        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    public static Texture2D MergeTexture(Texture2D texA, Texture2D texB, int width, int height) {
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

    public static Vector2[] ShrinkLeftUV(Vector2[] uv) {
        Vector2[] resultUV = new Vector2[uv.Length];
        int counter = 0;
        foreach(Vector2 val in uv)
            resultUV[counter++] = new Vector2(val.x/2, val.y);
        return resultUV;
    }

    public static Vector2[] ShrinkRightUV(Vector2[] uv) {
        Vector2[] resultUV = new Vector2[uv.Length];
        int counter = 0;
        foreach(Vector2 val in uv)
            resultUV[counter++] = new Vector2(val.x/2 + 0.5f, val.y);
        return resultUV;
    }

}