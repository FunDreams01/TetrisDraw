using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RawImage TetrisImage;
    public Camera TetrisCamera;
    

    // Start is called before the first frame update
    void Start()
    {
       var rtt = TetrisImage.GetComponent<RectTransform>();
       RenderTexture rt = TetrisImage.texture as RenderTexture;
       float w = rtt.rect.width;
       float h = rtt.rect.height;
       rt.width = (int) w;
       rt.height = (int) h;
       TetrisCamera.aspect =  w/h;
       TetrisCamera.ResetAspect();

       //rt.h
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
