    using System;
    using UnityEngine;

    public static class Noise
    {
        public enum  NormalizeMode
        {
            Local,
            Global
        }
        
        public static float[,] GenerateNosisMap(int mapWidth, int mapHeight, int seed, float scale, int octaves,
            float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            System.Random prng = new System.Random(seed);
            Vector2[] octavesOffsets = new Vector2[octaves];
            float maxPossibleHeight = 0;
            float amplitude = 1;    // 振幅
            float frequency = 1;    // 频率

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000,100000) + offset.x;
                float offsetY= prng.Next(-100000,100000) + offset.y;
                octavesOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }
            if (scale <= 0)
            {
                scale = 0.001f;
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;

            // 改变振幅是  永远是中心进行缩放
            float halfWidth = mapWidth / 2f;
            float halfHeight = mapHeight / 2f;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    amplitude = 1;    // 振幅
                    frequency = 1;    // 频率
                    
                    float noiseHeight = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octavesOffsets[i].x) / scale * frequency;
                        float sampleY = (y - halfHeight + octavesOffsets[i].y) / scale * frequency;
                        
                        // 将（0，1）的范围修改到  （-1，1）范围
                        float perlineValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;  
                        noiseHeight += perlineValue * amplitude; // Y的采样频率越高  采样的点就越远  意味着高度变化越快

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }


            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (normalizeMode == NormalizeMode.Local)
                    {
                        noiseMap[x,y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x,y]);    
                    }
                    else
                    {
                        float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                        noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                    }

                }
            }

            return noiseMap;
        }


    }
    