using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestForTouch : MonoBehaviour
{
   
   public RectTransform go;
   bool t = false;
   bool t2 = false;
    // Update is called once per frame
    void Update()
    {
       if(Input.GetMouseButton(0))
       {
           go.position =  Input.mousePosition;
       } 
    }

    public void Test()
    {
    if(t2)
     {go.sizeDelta *=2 ;t2=false;}
     else
     {
     go.sizeDelta *= 0.5f ; t2=true;
     }
    }

    public void Test2()
    {
     if(t)
     {go.GetComponent<Image>().color = Color.yellow;t=false;}
     else
     {
     go.GetComponent<Image>().color = Color.blue; t=true;
     }
    }
}
