using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   [HideInInspector] public Transform SpawnLocation;

   private void Awake() {
       GameObject go = new GameObject("SpawnLocation");
       go.transform.position = Vector3.zero;
       SpawnLocation = go.transform;
   }
}
