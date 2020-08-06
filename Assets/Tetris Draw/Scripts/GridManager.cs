using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("Panel Information")]
    public Vector2Int Dimensions;
    public uint DisabledTileCount;
    public RectTransform DrawPanel;
    [Header("Tile Settings")]
    public Sprite GridTileSprite;
    public float TileSpriteMultiplier;
    public Color DefaultColor, SelectedColor, DisabledColor;
    public Texture2D[] CorrespondingImages;

    [Header("Arrow Settings")]
    public Sprite ArrowEndPointSprite;
    public float ArrowBaseWidth;

    public Color ArrowEndPointColor, ArrowBaseColor;


    [Header("Block Settings")]
    public GameObject BlockPrefab;
    [HideInInspector] public TetrisBlockHolder BlockHolder, UpperBlockHolder;

    public Material DisabledBlockMaterial, SilhuetteMaterial;



    //PRIVATE VARIABLES
    Material og_BlockMaterial;
    [HideInInspector] public GameObject SilhuettePrefab;
    GameManager gameManager;
    [HideInInspector] public UIManager uIManager;
    private Stack<GridTile> DrawStack;
    private GridTile LastDrawnTile;
    private List<GridTile> AllTiles;
    [HideInInspector] public bool isDrawing;
    private Stack<Arrow> Arrows;
    private Arrow CurrentArrow;

    LevelManager levelManager;
    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        uIManager = FindObjectOfType<UIManager>();
        levelManager = FindObjectOfType<LevelManager>();
        FindObjectOfType<UIManager>().HaloWidthInBlocks = Dimensions.x;
        int size = Dimensions.x * Dimensions.y;
        AllTiles = new List<GridTile>(size);
        Arrows = new Stack<Arrow>(size);
        Vector2 TileDimensions = DrawPanel.rect.size / ((Vector2)Dimensions);
        for (int i = 0; i < Dimensions.x; i++)
        {
            for (int j = 0; j < Dimensions.y; j++)
            {
                AllTiles.Add(CreateTile(i, j, TileDimensions, ArrowEndPointColor));
            }
        }
        DrawStack = new Stack<GridTile>(size);
    }
    void Start()
    {
     //   DisableRandomly();
    }

    public void InitializeBlock()
    {
        BlockPrefab = Instantiate(BlockPrefab, SpaceConversionUtility.GridSpaceToWorldSpace(new Vector2(-10, -10)), Quaternion.identity);
        BlockPrefab.name = "Temporary Block Prefab Copy";
        MeshRenderer BlockMeshRenderer = BlockPrefab.GetComponentInChildren<MeshRenderer>();
        og_BlockMaterial = BlockMeshRenderer.material;
        BlockMeshRenderer.material = DisabledBlockMaterial;
        SilhuettePrefab = Instantiate(BlockPrefab);
        SilhuettePrefab.name = "Temporary Silhuette Prefab";
        SilhuettePrefab.GetComponentInChildren<MeshRenderer>().material = SilhuetteMaterial;

    }

   
    public void DisableRandomly()
    {
        int disabled = 0;
        for (int i = 0; i < AllTiles.Count; i++)
        {
            int left = AllTiles.Count - i;
            float probability = (float)(DisabledTileCount - disabled) / (float)left;
            float rand = Random.Range(0f, 1f);
            if (rand < probability)
            {
                AllTiles[i].ChangeDraw(GridTile.STATE.DISABLED);
                disabled++;
            }
        }
    }
    public void EnableAll()
    { for (int i = 0; i < AllTiles.Count; i++)
        {
                AllTiles[i].ChangeDraw(GridTile.STATE.AVAILABLE);
        }
    }

    public void ShuffleTileTextures()
    {
    foreach(GridTile gt in AllTiles) gt.SetBlockPreviewIndex(Random.Range(0,CorrespondingImages.Length));
    }

    GridTile CreateTile(int x, int y, Vector2 TileDimension, Color color)
    {
        GameObject TileObject = new GameObject("Grid (" + x + " , " + y + ")", typeof(RectTransform));
        RectTransform rt = TileObject.GetComponent<RectTransform>();
        rt.SetParent(DrawPanel, true);
        Vector2 top_left_zero = new Vector2(0, 1f);
        rt.anchorMin = top_left_zero;
        rt.anchorMax = top_left_zero;
        rt.pivot = top_left_zero;
        rt.localScale = Vector3.one;
        rt.sizeDelta = TileDimension;
        rt.anchoredPosition = new Vector2(x * TileDimension.x, -y * TileDimension.y);
        TileObject.AddComponent<Text>();
        GameObject TileImageChild = new GameObject("Tile Image Child", typeof(RectTransform));
        TileImageChild.transform.SetParent(TileObject.transform);
        RectTransform rt2 = TileImageChild.GetComponent<RectTransform>();
        rt2.anchoredPosition = Vector2.zero;
        rt2.sizeDelta = TileDimension * TileSpriteMultiplier;
        TileImageChild.transform.localScale = Vector3.one;
        Image im = TileImageChild.AddComponent<Image>();
        im.sprite = GridTileSprite;
        im.color = DefaultColor;
        im.raycastTarget = false;
        TileImageChild.AddComponent<Mask>().showMaskGraphic = false;



        GameObject GeneralImage = Instantiate(TileImageChild);
        GeneralImage.transform.SetParent(TileImageChild.transform,false);
        Destroy(GeneralImage.GetComponent<Mask>());


        GameObject BlockPreviewGO = new GameObject("Block Preview Texture", typeof(RectTransform), typeof(RawImage));
        BlockPreviewGO.transform.SetParent(TileImageChild.transform,false);
        RectTransform rt3 = BlockPreviewGO.GetComponent<RectTransform>();
        rt3.anchorMin = Vector2.zero;
        rt3.anchorMax = Vector2.one;
        rt3.pivot = new Vector2(0.5f,0.5f);
        rt3.offsetMin = Vector2.zero;
        rt3.offsetMax = Vector2.zero;

        rt3.SetAsFirstSibling();


        GridTile gt = TileObject.AddComponent<GridTile>();
        gt.Coordinates = new Vector2Int(x, y);
        gt.ArrowColor = color;
        gt.gridManager = this;
        gt.CenterOffset = new Vector2(TileDimension.x / 2f, -TileDimension.y / 2f);
        gt.ImageObjectImage = GeneralImage.GetComponent<Image>();
        gt.BlockPreview = BlockPreviewGO.GetComponent<RawImage>();
        gt.BlockPreview.enabled = false;
        gt.Init();
        gt.SetBlockPreviewIndex(Random.Range(0,CorrespondingImages.Length));
        return gt;
    }

    void UpdateCollidingTile()
    {
        int minDist = int.MinValue;
        GridTile minGT = null;
        foreach (GridTile gt in DrawStack)
        {
            int dist = levelManager.TopGridIndexPerColumn[gt.Block.GetComponent<TetrisBlock>().Coordx] + gt.Block.GetComponent<TetrisBlock>().localCoordy;
            if (dist > minDist)
            {
                minGT = gt;
                minDist = dist;
            }
        }



        int last_dist = levelManager.TopGridIndexPerColumn[LastDrawnTile.Block.GetComponent<TetrisBlock>().Coordx] + LastDrawnTile.Block.GetComponent<TetrisBlock>().localCoordy;
        if (last_dist > minDist)
        {
            minGT = LastDrawnTile;
        }
        BlockHolder.CollidingTile = minGT;
    }


    public void StartDraw()
    {
        if(!gameManager.isPlaying) return;
        isDrawing = true;
        if (BlockHolder == null)
        {
            GameObject BlockHolderGO = new GameObject("BlockHolder");
            BlockHolderGO.transform.position = Vector3.zero;
            BlockHolderGO.transform.SetParent(gameManager.SpawnLocation, false);
            BlockHolder = BlockHolderGO.AddComponent<TetrisBlockHolder>();
        }
        if(UpperBlockHolder == null)
        {
            GameObject BlockHolderGO = new GameObject("UpperBlockHolder");
            BlockHolderGO.transform.position = Vector3.zero;
            BlockHolderGO.transform.SetParent(gameManager.SpawnLocation, false);
            UpperBlockHolder = BlockHolderGO.AddComponent<TetrisBlockHolder>();
        }
    }



    public void EndDraw()
    {

        FindObjectOfType<TestForTouch>().Test();
        RegisterLastDrawn();
        while (DrawStack.Count > 0)
        {
            GridTile gt = DrawStack.Pop();
            gt.ChangeDraw(GridTile.STATE.AVAILABLE);
        }
        while (Arrows.Count > 0)
        {
            Arrows.Pop().Kill();
        }
        if (CurrentArrow != null) { CurrentArrow.Kill(); CurrentArrow = null; }
        isDrawing = false;
    }

    void RegisterLastDrawn()
    {
        if (LastDrawnTile != null)
        {
            DrawStack.Push(LastDrawnTile);
            LastDrawnTile = null;
        }
    }

    void Update()
    {
        //SPAWN
        if (Input.GetMouseButtonUp(0) && isDrawing && gameManager.isPlaying)
        {
            SpawnBlock();
            EndDraw();
        }

        //CANCEL
        if (/* !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() || */ !gameManager.isPlaying || !SpaceConversionUtility.IsMouseOverRect(DrawPanel))
        {
            FindObjectOfType<TestForTouch>().Test2();
            EndDraw();
        }
        if (CurrentArrow != null)
        {

            var rt2 = DrawPanel.GetComponent<RectTransform>();
            Vector3 sizeDelta = new Vector3(rt2.sizeDelta.x / 2, -rt2.sizeDelta.y, 0);
            Vector2 pt;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(DrawPanel, Input.mousePosition, null, out pt);
//          Debug.Log((Vector3)pt);
            CurrentArrow.SetEnd((Vector3)pt + (sizeDelta));
            CurrentArrow.Draw();
        }

    }

    bool isTileAppropriate(GridTile tile)
    {
        if (tile.GetTileState() == GridTile.STATE.DISABLED)
        {
            return false;
        }

        bool app = true;
        if (LastDrawnTile != null)
        {
            if ((LastDrawnTile.Coordinates - tile.Coordinates).magnitude > 1)
            {
                app = false;
            }
        }
        return app;
    }


    public void RegisterToDrawBuffer(GridTile tile)
    {

        if (!isTileAppropriate(tile))
        {
            EndDraw();
            return;
        }
        if (DrawStack.Count > 0 && DrawStack.Peek() == tile)
        {
            GridTile gt2 = DrawStack.Pop();
            Arrow lastarr = Arrows.Pop();
            CurrentArrow.SetStart(lastarr.GetStartPos());
            lastarr.Kill();
            LastDrawnTile.ChangeDraw(GridTile.STATE.AVAILABLE);
            LastDrawnTile = gt2;
        }
        else if (DrawStack.Contains(tile))
        {
            return;
        }
        else
        {
            RegisterLastDrawn();
            LastDrawnTile = tile;
            tile.ChangeDraw(GridTile.STATE.SELECTED);

            if (CurrentArrow != null)
            {
                CurrentArrow.SetEnd(tile.GetCenterAnchoredPosition());
                CurrentArrow.Draw();
                Arrows.Push(CurrentArrow);
            }

            CurrentArrow = new Arrow(tile.GetCenterAnchoredPosition(), tile.GetCenterAnchoredPosition(), DrawPanel, ArrowBaseWidth, ArrowBaseColor);
        }

        UpdateCollidingTile();
        uIManager.MoveSpawnerOnly();
    }

    public void SpawnBlock()
    {

      //  if (UpperBlockHolder.transform.childCount < 2) return;
        UpperBlockHolder.transform.SetParent(null);
        //Vector3 newpos = BlockHolder.transform.position;
        UpperBlockHolder.transform.position = gameManager.SpawnLocation.position;
        UpperBlockHolder.isFree = true;
        UpperBlockHolder = null;
        Destroy(BlockHolder.gameObject);
        BlockHolder = null;
        RegisterLastDrawn();
        foreach (GridTile gt in DrawStack)
        {
            gt.Block = null;
            if (gt.UpperBlock != null)
            {
                MeshRenderer mr = gt.UpperBlock.GetComponentInChildren<MeshRenderer>();
                Texture thisTex = mr.material.mainTexture; 
                mr.material = og_BlockMaterial;
                mr.material.mainTexture = thisTex;
                gt.UpperBlock = null;
            }
            else
            {
                /*   Debug.LogError(gt.Coordinates);
                  Debug.Break(); */
            }
        }
    }
}


public class Arrow
{

    private Vector3 StartPos, EndPos;
    GameObject Line;
    Image CenterImg;

    float LineWidth;


    public void SetStart(Vector3 StartPosition)
    {

        StartPos = StartPosition;

    }

    public Vector3 GetStartPos() { return StartPos; }
    public Vector3 GetEndPos() { return EndPos; }

    public void SetEnd(Vector3 EndPosition)
    {

        EndPos = EndPosition;

    }
    public Arrow(Vector3 StartPosition, Vector3 EndPosition, RectTransform DrawPanel, float Thickness, Color color)
    {
        StartPos = StartPosition;
        EndPos = EndPosition;
        Line = new GameObject("Arrow Line", typeof(RectTransform));
        Line.transform.SetParent(DrawPanel.transform, true);
        CenterImg = Line.AddComponent<Image>();
        CenterImg.color = color;
        CenterImg.raycastTarget = false;
        LineWidth = Thickness;
        var rt = Line.GetComponent<RectTransform>();
        Vector2 top_left_zero = new Vector2(0, 1f);
        rt.anchorMin = top_left_zero;
        rt.anchorMax = top_left_zero;
        rt.pivot = new Vector3(0, 0.5f);
        rt.localScale = Vector3.one;// + new Vector3(0,0.2f,0);

    }

    public void Kill()
    {
        GameObject.Destroy(Line);
    }

    public void Draw()
    {
        var rt = Line.GetComponent<RectTransform>();
        Vector3 differenceVector = EndPos - StartPos;
        rt.sizeDelta = new Vector2(differenceVector.magnitude, LineWidth);
        float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
        Line.transform.localRotation = Quaternion.Euler(0, 0, angle);
        rt.anchoredPosition = new Vector3(StartPos.x, StartPos.y, CenterImg.transform.position.z);
    }
}



