using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   [HideInInspector] public Transform SpawnLocation;
   public float BlockDisappearDelay;
   public float BlockDropSpeedInitialAcceleration;
   public float BlockDropSpeedJerkMultiplier;
   public bool isPlaying = false;
   private void Awake() {
       GameObject go = new GameObject("SpawnLocation");
       go.transform.position = Vector3.zero;
       SpawnLocation = go.transform;
       TetrisBlockHolder.JerkMultiplier = BlockDropSpeedJerkMultiplier;
       TetrisBlockHolder.InitialAcceleration = BlockDropSpeedInitialAcceleration;
   }
}