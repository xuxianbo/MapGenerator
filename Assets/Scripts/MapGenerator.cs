using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    public enum DrawNode { NoiseMap, ColorMap, Mesh, FalloffMap};

    public DrawNode drawNode;

    public Noise.NormalizeMode normalizeMode;
    
    /// <summary>
    /// 应该unity 中单个模型最大定点数 65535个
    /// w<=255
    /// w-1=240      能够被 [2, 4 , 6, 8, 10, 12] 之间的所有数整除
    /// w=241
    /// </summary>
    public const int mapChunkSize = 241;  // 真实生成的网格数会比它小一个  240*240
    [FormerlySerializedAs("levelOfDetail")] [Range(0, 6)]
    public int editorPreviewLOD;
    
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 Offset;

    public bool useFallOff;
    
    [Tooltip("模型的高度倍速")]
    // [Range(1, 10)]
    public float heightMultiplier;

    public AnimationCurve meshHegithCurve;
    
    public bool autoUpdate;

    public TerrainType[] Regions;

    private float[,] falloffMap;
    
    private Queue<MapThreadInfo<MapData>> mapDataThreadInfosQueue = new Queue<MapThreadInfo<MapData>>();
    private Queue<MapThreadInfo<MeshData>> meshDataThreadInfosQueue = new Queue<MapThreadInfo<MeshData>>();

    private void Awake()
    {
        falloffMap = FalloffGennerator.GenerateFalloffMap(mapChunkSize);
    }

    public void DrawMapEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        if (drawNode == DrawNode.NoiseMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFormHeightMap(mapData.heightMap));
        }
        else if (drawNode == DrawNode.ColorMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawNode == DrawNode.Mesh)
        {
            mapDisplay.DrawMesh(
                MeshGenertor.GeneratorTerrainMesh(mapData.heightMap, heightMultiplier, meshHegithCurve, editorPreviewLOD),
                TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }else if (drawNode == DrawNode.FalloffMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFormHeightMap(FalloffGennerator.GenerateFalloffMap(mapChunkSize)));
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callBack)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callBack);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callBack)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfosQueue)
        {
            mapDataThreadInfosQueue.Enqueue(new MapThreadInfo<MapData>(callBack, mapData));    
        }
    }

    public void RequesMeshData(MapData mapData, int lod, Action<MeshData> callBack)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callBack);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod ,Action<MeshData> callback)
    {
        MeshData meshData = MeshGenertor.GeneratorTerrainMesh(mapData.heightMap, heightMultiplier, meshHegithCurve, lod);
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

    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNosisMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center+Offset, normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFallOff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
                float currHeight = noiseMap[x, y];
                for (int i = 0; i < Regions.Length; i++)
                {
                    if (currHeight>=Regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = Regions[i].Color;
                    }
                    else
                    {
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

        falloffMap = FalloffGennerator.GenerateFalloffMap(mapChunkSize);
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