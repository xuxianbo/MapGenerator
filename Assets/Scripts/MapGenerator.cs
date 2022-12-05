using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawNode { NoiseMap, ColorMap};

    public DrawNode drawNode;
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 Offset;
    
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
