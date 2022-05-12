using UnityEngine;

public static class TextureGenerator {
    public static Texture2D CreateTextureByNoiseMap(
        float[,] heightMap,
        AnimationCurve colorCurve,
        Color colorLow,
        Color colorHigh
    ) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[texture.width * texture.height];
        for(int y = 0; y < height; ++y) {
            for(int x = 0; x < width; ++x) {
                colorMap[y*width+x] = Color.LerpUnclamped(
                    colorLow, 
                    colorHigh,
                    colorCurve.Evaluate(heightMap[x, y])
                );
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    public static Texture2D CreateTextureMonoColor(
        int width,
        int height,
        Color color
    ) {
        Texture2D texture = new Texture2D(width, height);
        Color[] colorMap = new Color[texture.width * texture.height];

        for(int y = 0; y < height; ++y)
            for(int x = 0; x < width; ++x)
                colorMap[y*width+x] = color;

        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }
}