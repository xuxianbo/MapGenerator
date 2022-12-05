using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EndlessTerrain: MonoBehaviour
{
    public const float maxViewDst = 450;
    public Transform viewer;

    public static Vector2 viewerPosition;
    private int chunkSize;
    private int chunkVisibleInViewDst;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDIctionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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
                    terrainChunkDIctionary.Add(viewedVhunkCoord, new TerrainChunk(viewedVhunkCoord, chunkSize, transform));
                }
            }
        }
    }




    public class TerrainChunk
    {
        private GameObject meshGameObject;
        private Vector2 position;
        private Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform prent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 pos = new Vector3(position.x, 0, position.y);

            meshGameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshGameObject.transform.position = pos;
            meshGameObject.transform.localScale = Vector3.one * size / 10f;
            meshGameObject.transform.SetParent(prent);
            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);
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
}


