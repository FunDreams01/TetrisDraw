﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Level Design")]
    public int ScreenWidthInBlocks;
    public GameObject[] BlockPrefabs;

    [Header("UI Settings")]
    public float HaloTransparency;
    public Color HaloColor;
    [HideInInspector] public int HaloWidthInBlocks;
    public float SpawnerOffsetFromTop;

    [Header("Configuration, Be Careful")]
    public RawImage TetrisImage;
    public Camera TetrisCamera;
    public RectTransform HaloParent;

    //PRIVATE VARS
    GameManager gameManager;
    RectTransform Halo;

    //UNITY FUNCTIONS

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        SpaceConversionUtility.ScreenWidthInBlocks = ScreenWidthInBlocks;
    }

    private void OnDrawGizmos()
    {

        if(!Application.isPlaying || Time.frameCount < 10) return;
        for (int i = 0; i < ScreenWidthInBlocks; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(i, j)), SpaceConversionUtility.BlockWidth * Vector3.one);
            }
        }
    }
    void Update()
    {
        if (Time.frameCount == 1)
        {
            AdjustRenderStuff();
            GenerateGrid();
            GenerateHaloImage();
            MoveHaloAndSpawner(Halo.anchoredPosition);
            FindObjectOfType<GridManager>().InitializeBlock();
            FindObjectOfType<LevelManager>().AddLevel();
        }
        else if (Time.frameCount > 1)
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 outpos;
                if (SpaceConversionUtility.ScreenSpaceToRectSpace(Input.mousePosition, out outpos))
                {
                    outpos.x -= SpaceConversionUtility.CanvasScale.x * Halo.sizeDelta.x;
                    MoveHaloAndSpawner(outpos);
                }
            }
        }
    }


    [HideInInspector] public int Coordx; 
    //UI FUNCTIONS
    public void MoveHaloAndSpawner(Vector3 RectCoords)
    {
        Halo.anchoredPosition = new Vector2(RectCoords.x, 0);
        Halo.anchoredPosition = SpaceConversionUtility.SnapRectPosToGridRectPos(Halo.anchoredPosition, out Coordx, 0, SpaceConversionUtility.ScreenWidthInBlocks - HaloWidthInBlocks);
        Vector3 v = gameManager.SpawnLocation.position;
        v = SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(Coordx, SpaceConversionUtility.ScreenHeightInBlocks - SpawnerOffsetFromTop));
        gameManager.SpawnLocation.position = v;
    }

    public void AdjustRenderStuff()
    {
        SpaceConversionUtility.TetrisScreenBounds = SpaceConversionUtility.RectTransformToScreenSpace(TetrisImage.rectTransform);
        float w = SpaceConversionUtility.TetrisScreenBounds.width;
        float h = SpaceConversionUtility.TetrisScreenBounds.height;
        if (TetrisCamera.targetTexture != null)
        {
            TetrisCamera.targetTexture.Release();
        }
        TetrisCamera.targetTexture = new RenderTexture((int)w, (int)h, 24);
        TetrisImage.texture = TetrisCamera.targetTexture;
        SpaceConversionUtility.CanvasScale = TetrisImage.transform.lossyScale;
        // TetrisCamera.aspect = w / h;
        // TetrisCamera.ResetAspect();
    }

    public void GenerateGrid()
    {
        float dist = Vector3.Dot((gameManager.SpawnLocation.position - TetrisCamera.transform.position), TetrisCamera.transform.forward);
        Vector3 BottomLeftCorner = TetrisCamera.ViewportToWorldPoint(new Vector3(0, 0, dist));
        Vector3 BottomRightCorner = TetrisCamera.ViewportToWorldPoint(new Vector3(1, 0, dist));
        Vector3 TopLeftCorner = TetrisCamera.ViewportToWorldPoint(new Vector3(0, 1, dist));
        SpaceConversionUtility.BlockWidth = (BottomRightCorner - BottomLeftCorner).magnitude;
        SpaceConversionUtility.BlockWidth /= (float)ScreenWidthInBlocks;
        SpaceConversionUtility.ScreenHeightInBlocks = (int)(((TopLeftCorner - BottomLeftCorner).magnitude) / SpaceConversionUtility.BlockWidth);
        foreach (GameObject BlockPrefab in BlockPrefabs)
        { BlockPrefab.transform.localScale = SpaceConversionUtility.BlockWidth * Vector3.one; }
        SpaceConversionUtility.LeftDir = (BottomRightCorner - BottomLeftCorner).normalized * SpaceConversionUtility.BlockWidth;
        SpaceConversionUtility.UpDir = (TopLeftCorner - BottomLeftCorner).normalized * SpaceConversionUtility.BlockWidth;
        SpaceConversionUtility.BottomLeftSpawnPos = BottomLeftCorner + (SpaceConversionUtility.LeftDir / 2f) + (SpaceConversionUtility.UpDir / 2f);
    }


    void GenerateHaloImage()
    {
        Texture2D tex = new Texture2D(1, TetrisImage.texture.height, TextureFormat.ARGB32, false);
        for (int i = 0; i < tex.height; i++)
        {
            Color c = new Color(HaloColor.r, HaloColor.g, HaloColor.b, Mathf.Pow(((float)i / tex.height), HaloTransparency));
            if (c.a < 0.01) c = new Color(0f, 0f, 0f, 0f);
            tex.SetPixel(0, i, c);
        }
        tex.Apply();
        GameObject go = new GameObject("Halo");
        RawImage im = go.AddComponent<RawImage>();
        im.texture = tex;
        Halo = im.rectTransform;
        Halo.SetParent(HaloParent, false);
        Vector2 top_left_zero = new Vector2(0, 1f);
        Halo.anchorMin = top_left_zero;
        Halo.anchorMax = top_left_zero;
        Halo.pivot = top_left_zero;
        Halo.localScale = Vector3.one;
        Halo.anchoredPosition = new Vector2(0, 0);
        Halo.sizeDelta = new Vector2(HaloWidthInBlocks * (SpaceConversionUtility.TetrisScreenBounds.width / (SpaceConversionUtility.CanvasScale.x * ScreenWidthInBlocks)), tex.height / (SpaceConversionUtility.CanvasScale.y));

    }

}
