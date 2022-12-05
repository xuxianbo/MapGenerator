using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    public enum DrawNode { NoiseMap, ColorMap, Mesh};

    public DrawNode drawNode;

    /// <summary>
    /// 应该unity 中单个模型最大定点数 65535个
    /// w<=255
    /// w-1=240      能够被 [2, 4 , 6, 8, 10, 12] 之间的所有数整除
    /// w=241
    /// </summary>
    public const int mapChunkSize = 241;  // 真实生成的网格数会比它小一个  240*240
    [Range(0, 6)]
    public int levelOfDetail;
    
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 Offset;

    [Tooltip("模型的高度倍速")]
    [Range(1, 10)]
    public float heightMultiplier;

    public AnimationCurve meshHegithCurve;
    
    public bool autoUpdate;

    public TerrainType[] Regions;
    
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNosisMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity,Offset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currHeight = noiseMap[x, y];
                for (int i = 0; i < Regions.Length; i++)
                {
                    if (currHeight<=Regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = Regions[i].Color;
                        break;
                    }
                }
            }
        }
        MapDisplay mapDisplay=FindObjectOfType<MapDisplay>();
        if (drawNode == DrawNode.NoiseMap)
        {
            mapDisplay.DrawNoiseMap(TextureGenerator.TextureFormHeightMap(noiseMap));    
        }else if (drawNode == DrawNode.ColorMap)
        {
            mapDisplay.DrawNoiseMap(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }else if (drawNode == DrawNode.Mesh)
        {
            mapDisplay.DrawMesh(MeshGenertor.GeneratorTerrainMesh(noiseMap, heightMultiplier, meshHegithCurve, levelOfDetail),
                TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }
    }


    private void OnValidate()
    {
        if (lacunarity<1)
        {
            lacunarity = 1;
        }

        if (octaves<0)
        {
            octaves = 0;
        }
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color Color;
}
