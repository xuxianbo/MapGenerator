using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawNode { NoiseMap, ColorMap, Mesh};

    public DrawNode drawNode;

    private const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail;
    
    public int mapWidth;
    public int mapHeight;
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
        float[,] noiseMap = Noise.GenerateNosisMap(mapHeight, mapWidth, seed, noiseScale, octaves, persistance, lacunarity,Offset);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currHeight = noiseMap[x, y];
                for (int i = 0; i < Regions.Length; i++)
                {
                    if (currHeight<=Regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = Regions[i].Color;
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
            mapDisplay.DrawNoiseMap(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }else if (drawNode == DrawNode.Mesh)
        {
            mapDisplay.DrawMesh(MeshGenertor.GeneratorTerrainMesh(noiseMap, heightMultiplier, meshHegithCurve, levelOfDetail),
                TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
    }


    private void OnValidate()
    {
        if (mapWidth<1)
        {
            mapWidth = 1;
        }

        if (mapHeight<1)
        {
            mapHeight = 1;
        }

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
