using System.Collections;
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
    [Header ("Screen Shake")]
    public float ShakeSpeed;
    public float ShakeAmount, ShakeDuration, ShakeDecayInverseSmoothness;

    [Header("Configuration, Be Careful")]
    public RawImage TetrisImage;
    public Transform PlayArea;
    public Camera TetrisCamera;
    public RectTransform HaloParent;
    public GameObject WinScreen, LoseScreen;
    public GameObject LoseBar;

    //PRIVATE VARS
    GameManager gameManager;
    LevelManager levelManager;
    RectTransform Halo;
    GridManager gridManager;
    Vector2 ScreenStartPos;

    bool ShakeIt;
    //UNITY FUNCTIONS

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        SpaceConversionUtility.ScreenWidthInBlocks = ScreenWidthInBlocks;
        levelManager = FindObjectOfType<LevelManager>();
        gridManager = FindObjectOfType<GridManager>();
    }


    int multiplier = 1;
    float ShakeFinishTime;
    public void ShakeItUp()
    {
        if (ShakeIt)
        {
            multiplier++;
            multiplier++;
            ShakeFinishTime = Time.time + (ShakeDuration * multiplier);

        }
        else
        {
            multiplier = 1;
            ShakeFinishTime = Time.time + ShakeDuration;
        }
        ShakeIt = true;
    }

    private void OnDrawGizmos()
    {

        if (!Application.isPlaying || Time.frameCount - StartFrame < 10) return;
        for (int i = 0; i < ScreenWidthInBlocks; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(i, j)), SpaceConversionUtility.BlockWidth * Vector3.one);
            }
        }
    }

    public void Win()
    {
        WinScreen.SetActive(true);
    }

    public void Lose()
    {
        LoseScreen.SetActive(true);
    }

    public void GameScreen()
    {
        WinScreen.SetActive(false);
        LoseScreen.SetActive(false);
    }

    void Update()
    {
        if (Time.frameCount - StartFrame == 1)
        {
            AdjustRenderStuff();
            GenerateGrid();
            GenerateHaloImage();
            MoveHaloAndSpawner(Halo.anchoredPosition);
            PositionLoseBar();
            FindObjectOfType<GridManager>().InitializeBlock();

            levelManager.Initialize();
        }
        else if (Time.frameCount - StartFrame > 1)
        {
            if (Input.GetMouseButton(0) && gameManager.isPlaying)
            {
                Vector2 outpos;
                if (SpaceConversionUtility.ScreenSpaceToRectSpace(Input.mousePosition, out outpos))
                {
                    outpos.x -= SpaceConversionUtility.CanvasScale.x * Halo.sizeDelta.x;
                    MoveHaloAndSpawner(outpos);
                }
            }

            if (ShakeIt)
            {
                if (Time.time > ShakeFinishTime)
                {
                    ShakeIt = false;
                    PlayArea.position = ScreenStartPos;
                }
                else
                {
                    float TimeLeft = ShakeFinishTime - Time.time;
                    float decay = Mathf.Pow(TimeLeft, ShakeDecayInverseSmoothness);
                    Vector3 ppos = PlayArea.position;
                    ppos.x = ScreenStartPos.x + Mathf.Sin(Time.time * ShakeSpeed) * ShakeAmount * multiplier * decay;
                    ppos.y = ScreenStartPos.y + (Mathf.Sin(Time.time * ShakeSpeed) * ShakeAmount * multiplier * decay);
                    PlayArea.position = ppos;
                }
            }
        }
    }

    [HideInInspector] public int Coordx;
    //UI FUNCTIONS


    public void MoveSpawnerOnly()
    {
        if (gridManager.isDrawing && gridManager.BlockHolder != null && gridManager.BlockHolder.CollidingTile != null && gridManager.BlockHolder.CollidingTile.Block != null)
        {

            foreach (var t in FindObjectsOfType<TetrisBlockHolder>())
            {
                if (t.isFree)
                {
                    Transform first = gridManager.BlockHolder.transform;
                    for (int i = 0; i < first.childCount; i++)
                    {
                        Transform second = t.transform;
                        for (int j = 0; j < second.childCount; j++)
                        {
                            if (first.GetChild(i).GetComponent<TetrisBlock>().Coordx == second.GetChild(j).GetComponent<TetrisBlock>().Coordx)
                            {
                                gridManager.BlockHolder.gameObject.SetActive(false);
                                return;
                            }
                        }
                    }
                }
            }

            if (!gridManager.BlockHolder.gameObject.activeSelf) gridManager.BlockHolder.gameObject.SetActive(true);
            TetrisBlock tetrisBlock = gridManager.BlockHolder.CollidingTile.Block.GetComponent<TetrisBlock>();
            //    Debug.Log(tetrisBlock.Coordx + " " + tetrisBlock.localCoordy);
            int yForTB = levelManager.TopGridIndexPerColumn[tetrisBlock.Coordx] + 1;
            Vector3 newPosforTB = SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(tetrisBlock.Coordx, yForTB));
            Vector3 disp = newPosforTB - tetrisBlock.transform.position;
            gridManager.BlockHolder.transform.position = gridManager.BlockHolder.transform.position + disp;
        }
    }
    public void MoveHaloAndSpawner(Vector3 RectCoords)
    {
        if (gridManager.isDrawing)
        {
            gridManager.EndDraw();
        }
        Halo.anchoredPosition = new Vector2(RectCoords.x, 0);
        Halo.anchoredPosition = SpaceConversionUtility.SnapRectPosToGridRectPos(Halo.anchoredPosition, out Coordx, 0, SpaceConversionUtility.ScreenWidthInBlocks - HaloWidthInBlocks);
        Vector3 v = gameManager.SpawnLocation.position;

        v = SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(Coordx, SpaceConversionUtility.ScreenHeightInBlocks - SpawnerOffsetFromTop));

        //  v = SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(Coordx, ));
        gameManager.SpawnLocation.position = v;
        MoveSpawnerOnly();
    }

    public void AdjustRenderStuff()
    {
        ScreenStartPos.x = PlayArea.transform.position.x;
        ScreenStartPos.y = PlayArea.transform.position.y;
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
    int StartFrame = 0;
    void Start()
    {
        StartFrame = Time.frameCount;

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
        Halo.SetSiblingIndex(1);
    }

    void PositionLoseBar()
    {
        LoseBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, ((SpaceConversionUtility.TetrisScreenBounds.width / ScreenWidthInBlocks) / SpaceConversionUtility.CanvasScale.y) * levelManager.LoseLevels);
    }

}
