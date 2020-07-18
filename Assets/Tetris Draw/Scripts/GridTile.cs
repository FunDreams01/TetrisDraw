using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridTile : MonoBehaviour
{
    public Vector2Int Coordinates;
    public GridManager gridManager;
    private bool selected;

    public Vector2 CenterOffset;

    private GameObject ArrowEndPoint;

    public Color ArrowColor;
    public void ChangeDraw(bool isSelected)
    {
        selected = isSelected;
        ArrowEndPoint.SetActive(isSelected);
        if(isSelected)
        {
            GetComponent<Image>().color = gridManager.SelectedColor;
        }
        else
        {
            GetComponent<Image>().color = gridManager.DefaultColor;
        }

    }

    public Vector2 GetCenterAnchoredPosition () {
        
        return (Vector2) GetComponent<RectTransform>().anchoredPosition + CenterOffset;
    }

    GridTile(Color ArrowColor)
    {
        
    }

    void Start()
    {
        EventTrigger MyEventTrigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry PointerDownEntry = new EventTrigger.Entry();
        PointerDownEntry.eventID = EventTriggerType.PointerDown;
        PointerDownEntry.callback.AddListener((data) => { MyOnPointerDownDelegate((PointerEventData)data); });
        MyEventTrigger.triggers.Add(PointerDownEntry);

        EventTrigger.Entry PointerEnterEntry = new EventTrigger.Entry();
        PointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        PointerEnterEntry.callback.AddListener((data) => { MyOnPointerEnterDelegate((PointerEventData)data); });
        MyEventTrigger.triggers.Add(PointerEnterEntry);

        if(gridManager.ArrowEndPointSprite != null)
        {
            ArrowEndPoint = new GameObject(name + " ArrowEndPoint");
            ArrowEndPoint.transform.SetParent(transform,false);
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
        gridManager.isDrawing = true;
        gridManager.RegisterToDrawBuffer(this);
    }
    
    public void MyOnPointerEnterDelegate(PointerEventData data)
    {
        if(gridManager.isDrawing)
        {
        gridManager.RegisterToDrawBuffer(this);}
    }



}
