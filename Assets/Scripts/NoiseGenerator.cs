using UnityEngine;

public class NoiseGenerator {
    private const int PSEUDO_RANDOM_RANGE_MINIMUM = -100000;
    private const int PSEUDO_RANDOM_RANGE_MAXIMUM = 100000;

    public class Noise {
        private float[,] noiseMap;

        public Noise(float[,] noiseMap) {
            this.noiseMap = noiseMap;
        }

        public int GetSideLength() => noiseMap.GetLength(0);
        public float[,] Generate() => noiseMap;

        public Noise ApplySecondNoiseUnclamped(float[,] secondNoiseMap) {
            float minVal = float.MaxValue;

            for(int i = 0; i < GetSideLength(); ++i) {
                for(int j = 0; j < GetSideLength(); ++j) {
                    noiseMap[i, j] = noiseMap[i, j] - secondNoiseMap[i, j];
                    if(minVal > noiseMap[i, j]) minVal = noiseMap[i, j];
                }
            }
            for(int i = 0; i < GetSideLength(); ++i)
                for(int j = 0; j < GetSideLength(); ++j)
                    noiseMap[i, j] -= minVal;

            return this;
        }

        public Noise ApplySecondNoise(float[,] secondNoiseMap) {
            for(int i = 0; i < GetSideLength(); ++i)
                for(int j = 0; j < GetSideLength(); ++j)
                    noiseMap[i, j] = Mathf.Clamp01(noiseMap[i, j] - secondNoiseMap[i, j]);

            return this;
        }
    }

    public static Noise CreatePerlinNoise(int sideLength, int seed, float noiseScale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        return new Noise(GeneratePerlin(sideLength, seed, noiseScale, octaves, persistance, lacunarity, offset));
    }

    public static float[,] GenerateFalloff(int sideLength, AnimationCurve falloffCurve) {
        float[,] map = new float[sideLength,sideLength];
        for(int i = 0; i < sideLength; ++i) {
            for(int j = 0; j < sideLength; ++j) {
                float x = i / (float) sideLength * 2 - 1;
                float y = j / (float) sideLength * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i,j] = falloffCurve.Evaluate(value);
            }
        }
        return map;
    }

    public static float[,] GeneratePerlin(int sideLength, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[sideLength, sideLength];

        System.Random pRNG = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; ++i) {
            float offsetX = pRNG.Next(PSEUDO_RANDOM_RANGE_MINIMUM, PSEUDO_RANDOM_RANGE_MAXIMUM) + offset.x;
            float offsetY = pRNG.Next(PSEUDO_RANDOM_RANGE_MINIMUM, PSEUDO_RANDOM_RANGE_MAXIMUM) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if(scale <= 0) scale = 0.0001f;

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
                    
                    float perlinValue = Mathf.PerlinNoise(tempX, tempY) * 2 - 1;
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
}