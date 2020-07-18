using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("Panel Information")]
    public Vector2Int Dimensions;
    public RectTransform DrawPanel;
    [Header("Tile Settings")]
    public Sprite GridTileSprite;
    public Color DefaultColor, SelectedColor;

    [Header("Arrow Settings")]
    public Sprite ArrowEndPointSprite;
    public float ArrowBaseWidth;

    public Color ArrowEndPointColor, ArrowBaseColor;


    [Header("Block Settings")]
    public GameObject BlockPrefab;

    public Transform SpawnLocation;


    //PRIVATE VARIABLES
    private Stack<GridTile> DrawStack;
    private GridTile LastDrawnTile;
    private List<GridTile> AllTiles;
    [HideInInspector] public bool isDrawing;
    private Stack<Arrow> Arrows;
    private Arrow CurrentArrow;
    void Start()
    {
        AllTiles = new List<GridTile>(Dimensions.x * Dimensions.y);
        Arrows = new Stack<Arrow>(Dimensions.x * Dimensions.y);
        Vector2 TileDimensions = DrawPanel.rect.size / ((Vector2)Dimensions);
        for (int i = 0; i < Dimensions.x; i++)
        {
            for (int j = 0; j < Dimensions.y; j++)
            {
                AllTiles.Add(CreateTile(i, j, TileDimensions, ArrowEndPointColor));
            }
        }
        DrawStack = new Stack<GridTile>(Dimensions.x * Dimensions.y);
 


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
        Image im = TileObject.AddComponent<Image>();
        im.sprite = GridTileSprite;
        im.color = DefaultColor;
        GridTile gt = TileObject.AddComponent<GridTile>();
        gt.Coordinates = new Vector2Int(x, y);
        gt.ArrowColor = color;
        gt.gridManager = this;
        gt.CenterOffset = new Vector2(TileDimension.x / 2f, -TileDimension.y / 2f);
        return gt;
    }

    void EndDraw()
    {

        RegisterLastDrawn();
        while (DrawStack.Count > 0)
        {
            GridTile gt = DrawStack.Pop();
            gt.ChangeDraw(false);
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
        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            SpawnBlock(DrawStack);
            EndDraw();
        }

        //CANCEL
        if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            EndDraw();
        }
        if (CurrentArrow != null)
        {

            var rt2 = DrawPanel.GetComponent<RectTransform>();
            Vector3 sizeDelta = new Vector3(rt2.sizeDelta.x / 2, -rt2.sizeDelta.y / 2, 0);
            Vector2 pt;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(DrawPanel, Input.mousePosition, null, out pt);

            CurrentArrow.SetEnd((Vector3)pt + sizeDelta);
            CurrentArrow.Draw();
        }

    }

    bool isTileAppropriate(GridTile tile)
    {
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
            LastDrawnTile.ChangeDraw(false);
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
            tile.ChangeDraw(true);

            if (CurrentArrow != null)
            {
                CurrentArrow.SetEnd(tile.GetCenterAnchoredPosition());
                CurrentArrow.Draw();
                Arrows.Push(CurrentArrow);
            }

            CurrentArrow = new Arrow(tile.GetCenterAnchoredPosition(), tile.GetCenterAnchoredPosition(), DrawPanel, ArrowBaseWidth, ArrowBaseColor);
        }
    }

    public void SpawnBlock(IEnumerable<GridTile> DrawList)
    {
        
        GameObject BlockHolder = new GameObject("BlockHolder");
        Vector3 size = BlockPrefab.GetComponent<MeshRenderer>().bounds.size;
        RegisterLastDrawn();
        Vector2 CoordCenter = Vector2.zero;
        int c = 0;
        foreach (GridTile gt in DrawList)
        {
            c++;
            GameObject go = Instantiate(BlockPrefab, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(BlockHolder.transform, false);
            go.transform.position = new Vector3(gt.Coordinates.x *size.x, -gt.Coordinates.y*size.y, 0);
            CoordCenter += (Vector2) go.transform.position;
        }
        if(c!=0) CoordCenter /= c;
        BlockHolder.AddComponent<TetrisBlock>();
        BlockHolder.transform.position = SpawnLocation.position -  (Vector3) CoordCenter;
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



