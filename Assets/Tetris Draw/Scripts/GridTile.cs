﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridTile : MonoBehaviour
{
    public enum STATE
    {
        AVAILABLE,
        SELECTED,
        DISABLED
    }

    public Vector2Int Coordinates;
    public GridManager gridManager;
    private STATE TileState;

    public Vector2 CenterOffset;

    private GameObject ArrowEndPoint;

    public GameObject Block;
    public GameObject UpperBlock;

    public Color ArrowColor;

    GameManager gameManager;
    public Image ImageObjectImage;
    public void ChangeDraw(STATE NewState)
    {
        ArrowEndPoint.SetActive(NewState == STATE.SELECTED);
        switch (NewState)
        {
            case STATE.SELECTED:
                ImageObjectImage.color = gridManager.SelectedColor;
                if (Block == null)
                {
                    Block = Instantiate(gridManager.SilhuettePrefab, Vector3.zero, Quaternion.identity);
                    Vector3 pos = Block.transform.position;
                    pos += SpaceConversionUtility.LeftDir * Coordinates.x;
                    pos -= SpaceConversionUtility.UpDir * Coordinates.y;
                    Block.transform.position = pos;
                    Block.transform.SetParent(gridManager.BlockHolder.transform, false);
                    TetrisBlock tb = Block.gameObject.AddComponent<TetrisBlock>();
                }
                if (UpperBlock == null)
                {
                    UpperBlock = Instantiate(gridManager.BlockPrefab, Vector3.zero, Quaternion.identity);
                    Vector3 pos = UpperBlock.transform.position;
                    pos += SpaceConversionUtility.LeftDir * Coordinates.x;
                    pos -= SpaceConversionUtility.UpDir * Coordinates.y;
                    UpperBlock.transform.position = pos;
                    UpperBlock.transform.SetParent(gridManager.UpperBlockHolder.transform, false);
                    TetrisBlock tb = UpperBlock.gameObject.AddComponent<TetrisBlock>();
                }
                break;
            case STATE.AVAILABLE:
                if (Block != null)
                {
                    Destroy(Block.gameObject);
                    Block = null;
                }
                if(UpperBlock!=null)
                {
                    Destroy(UpperBlock.gameObject);
                    UpperBlock = null;
                }
                ImageObjectImage.color = gridManager.DefaultColor;
                break;
            case STATE.DISABLED:
                if (Block != null)
                {
                    Destroy(Block.gameObject);
                    Block = null;
                }
                if(UpperBlock!=null)
                {
                    Destroy(UpperBlock.gameObject);
                    UpperBlock = null;
                }
                ImageObjectImage.color = gridManager.DisabledColor;
                break;
        }
        if (Block != null)
        {

            TetrisBlock tb = Block.gameObject.GetComponent<TetrisBlock>();
            tb.Coordx = gridManager.uIManager.Coordx + Coordinates.x;
            tb.localCoordy = Coordinates.y;
        }
        if (UpperBlock != null)
        {
            TetrisBlock tb = UpperBlock.gameObject.GetComponent<TetrisBlock>();
            tb.Coordx = gridManager.uIManager.Coordx + Coordinates.x;
            tb.localCoordy = Coordinates.y;
        }
        TileState = NewState;
    }

    public STATE GetTileState() { return TileState; }

    public Vector2 GetCenterAnchoredPosition()
    {

        return (Vector2)GetComponent<RectTransform>().anchoredPosition + CenterOffset;
    }


    public void Init()
    {
        gameManager = FindObjectOfType<GameManager>();
        EventTrigger MyEventTrigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry PointerDownEntry = new EventTrigger.Entry();
        PointerDownEntry.eventID = EventTriggerType.PointerDown;
        PointerDownEntry.callback.AddListener((data) => { MyOnPointerDownDelegate((PointerEventData)data); });
        MyEventTrigger.triggers.Add(PointerDownEntry);

        EventTrigger.Entry PointerEnterEntry = new EventTrigger.Entry();
        PointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        PointerEnterEntry.callback.AddListener((data) => { MyOnPointerEnterDelegate((PointerEventData)data); });
        MyEventTrigger.triggers.Add(PointerEnterEntry);

        if (gridManager.ArrowEndPointSprite != null)
        {
            ArrowEndPoint = new GameObject(name + " ArrowEndPoint");
            ArrowEndPoint.transform.SetParent(transform, false);
            Image im = ArrowEndPoint.AddComponent<Image>();
            RectTransform rt = ArrowEndPoint.GetComponent<RectTransform>();
            im.sprite = gridManager.ArrowEndPointSprite;
            im.SetNativeSize();
            im.color = ArrowColor;
            ArrowEndPoint.SetActive(false);
        }
    }

    public void MyOnPointerDownDelegate(PointerEventData data)
    {
        if(!gameManager.isPlaying) return;
        gridManager.StartDraw();
        gridManager.RegisterToDrawBuffer(this);
    }

    public void MyOnPointerEnterDelegate(PointerEventData data)
    {
        if(!gameManager.isPlaying) return;
        if (gridManager.isDrawing)
        {
            gridManager.RegisterToDrawBuffer(this);
        }
    }



}
