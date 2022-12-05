using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public int mapSizeX;
    public int mapSizeY;
    
    public int Seed;
    
    [Range(0.1f, 10f)]
    public int Scale;

    [Range(0.1f, 1f)] public float HeightRange;
    
    [SerializeField]
    public GameObject MapCell;

    private Grid mapGrid;
    // Start is called before the first frame update
    void Start()
    {
        mapGrid = this.GetComponent<Grid>();
        if (Seed == 0)
        {
            Seed = (int) System.DateTime.Now.Ticks;
        }
        Random.InitState(Seed);
        float orix = Random.Range(0.0f, 999.9f);
        float oriy = Random.Range(0.0f, 999.9f);
        float xSample = 0;
        float ySample = 0;

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                xSample = (orix + x) / mapSizeX * Scale;
                ySample = (oriy + y) / mapSizeY * Scale;
                float height = Mathf.PerlinNoise(xSample, ySample);

                if (height > HeightRange)
                {
                    GameObject go = Instantiate(MapCell, mapGrid.CellToWorld(new Vector3Int(x, 0, y)), Quaternion.identity) as GameObject;
                    
                    Vector3 pos = go.transform.position;
                    pos.y = height * 20;
                    go.transform.position = pos;

                }
            }
        }
    }

}
