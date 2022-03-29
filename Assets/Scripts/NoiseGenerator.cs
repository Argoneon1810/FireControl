using UnityEngine;

public class NoiseGenerator {
    public static float[,] Generate(int sideLength, int seed, float noiseScale, int octaves, float persistance, float lacunarity, Vector2 offset, bool useFalloff, bool clampFalloff, AnimationCurve falloffCurve) {
        float[,] noiseMap = GeneratePerlin(sideLength, seed, noiseScale, octaves, persistance, lacunarity, offset);
        if(useFalloff) {
            float minVal = float.MaxValue;
            float[,] falloffMap = GenerateFalloff(sideLength, falloffCurve);
            for(int i = 0; i < sideLength; ++i) {
                for(int j = 0; j < sideLength; ++j) {
                    if(clampFalloff)
                        noiseMap[i, j] = Mathf.Clamp01(noiseMap[i, j] - falloffMap[i, j]);
                    else {
                        noiseMap[i, j] = noiseMap[i, j] - falloffMap[i, j];
                        if(minVal > noiseMap[i, j]) minVal = noiseMap[i, j];
                    }
                }
            }
            if(!clampFalloff)
                for(int i = 0; i < sideLength; ++i)
                    for(int j = 0; j < sideLength; ++j)
                        noiseMap[i, j] -= minVal;
        }
        return noiseMap;
    }

    public static float[,] GeneratePerlin(int sideLength, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[sideLength, sideLength];

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

        float halfLength = sideLength / 2;

        for(int y = 0; y < sideLength; ++y) {
            for(int x = 0; x < sideLength; ++x) {
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

        for(int y = 0; y < sideLength; ++y) {
            for(int x = 0; x < sideLength; ++x) {
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
            }
        }

        return noiseMap;
    }

    public static float[,] GenerateFalloff(int size, AnimationCurve falloffCurve) {
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

}