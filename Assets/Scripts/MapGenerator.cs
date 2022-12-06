using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

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

    private Queue<MapThreadInfo<MapData>> mapDataThreadInfosQueue = new Queue<MapThreadInfo<MapData>>();
    private Queue<MapThreadInfo<MeshData>> meshDataThreadInfosQueue = new Queue<MapThreadInfo<MeshData>>();
    public void DrawMapEditor()
    {
        MapData mapData = GenerateMap();
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        if (drawNode == DrawNode.NoiseMap)
        {
            mapDisplay.DrawNoiseMap(TextureGenerator.TextureFormHeightMap(mapData.heightMap));
        }
        else if (drawNode == DrawNode.ColorMap)
        {
            mapDisplay.DrawNoiseMap(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawNode == DrawNode.Mesh)
        {
            mapDisplay.DrawMesh(
                MeshGenertor.GeneratorTerrainMesh(mapData.heightMap, heightMultiplier, meshHegithCurve, levelOfDetail),
                TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }

    public void RequestMapData(Action<MapData> callBack)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callBack);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callBack)
    {
        MapData mapData = GenerateMap();
        lock (mapDataThreadInfosQueue)
        {
            mapDataThreadInfosQueue.Enqueue(new MapThreadInfo<MapData>(callBack, mapData));    
        }
    }

    public void RequesMeshData(MapData mapData, Action<MeshData> callBack)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, callBack);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenertor.GeneratorTerrainMesh(mapData.heightMap, heightMultiplier, meshHegithCurve, levelOfDetail);
        lock (meshDataThreadInfosQueue)
        {
            meshDataThreadInfosQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if (mapDataThreadInfosQueue.Count>0)
        {
            for (int i = 0; i < mapDataThreadInfosQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfosQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfosQueue.Count>0)
        {
            for (int i = 0; i < meshDataThreadInfosQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfosQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMap()
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

        return new MapData(noiseMap, colorMap);
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


    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
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

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}