﻿using UnityEngine;

public class MapDisplay : MonoBehaviour
{

    public Renderer textureRenderer;

    public void DrawNoiseMap(Texture2D texture )
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
