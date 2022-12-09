﻿using System.Collections.Generic;
using System.Net.Mime;
using Unity.VisualScripting;
using UnityEngine;

public class EndlessTerrain: MonoBehaviour
{
    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate =
        viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
        
    public LODInfo[] detailLevels;
    public static float maxViewDst;
    
    public Transform viewer;
    public Material mapMaterial;
    
    private static MapGenerator mapGenerator;
    public static Vector2 viewerPosition;
    private Vector2 viewerPositionOld;
    private int chunkSize;
    private int chunkVisibleInViewDst;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDIctionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        // 优化 只有当距离 范围发生设定改变时 重新刷新一次
        if ( (viewerPositionOld-viewerPosition).sqrMagnitude>sqrViewerMoveThresholdForChunkUpdate )
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDst; xOffset < chunkVisibleInViewDst; xOffset++)
            {
                Vector2 viewedVhunkCoord =
                    new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDIctionary.ContainsKey(viewedVhunkCoord))
                {
                    terrainChunkDIctionary[viewedVhunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDIctionary[viewedVhunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDIctionary[viewedVhunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDIctionary.Add(viewedVhunkCoord, new TerrainChunk(viewedVhunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }




    public class TerrainChunk
    {
        private GameObject meshGameObject;
        private Vector2 position;
        private Bounds bounds;

        
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        private LODInfo[] detailLevels;
        private LODMesh[] lodMeshes;

        private MapData mapData;
        private bool mapDataReceived;
        private int previousLODIndex=-1;
            
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform prent, Material material)
        {
            this.detailLevels = detailLevels;
            
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 pos = new Vector3(position.x, 0, position.y);

            meshGameObject = new GameObject("Terrain Chunk");
            meshRenderer = meshGameObject.AddComponent<MeshRenderer>();
            meshFilter = meshGameObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            
            meshGameObject.transform.position = pos;
            // meshGameObject.transform.localScale = Vector3.one * size / 10f;
            meshGameObject.transform.SetParent(prent);
            SetVisible(false);
            lodMeshes = new LODMesh[this.detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }
            
            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize,
                MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            
            UpdateTerrainChunk();
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.createMesh();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDst;
                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible)
        {
            meshGameObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshGameObject.activeSelf ;
        }
    }


    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;
        private System.Action updateCallback;
        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.createMesh();
            hasMesh = true;
            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequesMeshData(mapData, lod, OnMeshDataReceived);
        }
    }
    
    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
    }
}


