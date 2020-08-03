using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBlock : MonoBehaviour
{
    public int Coordx;
    MeshRenderer meshRenderer;
    // bool isSetForDestruction = false;
    //float destructionTime, startTime;
     void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    } 

   /*  void Start()
    {
        GetComponentInChildren<ParticleSystem>().Play();
    } */
    public void SetForDestructionDelayed(float delay)
    {
      /*   isSetForDestruction =true;
        startTime = Time.time;
        destructionTime = delay; */
        meshRenderer.enabled = false;
        Destroy(gameObject,delay);
    }

    public void PlayParticles()
    {
        GetComponentInChildren<BlockRandom>().PlayParticles();
    }
    

    void Update()
    {
      /*   if(isSetForDestruction)
        {
        meshRenderer.material.color = Color.Lerp(meshRenderer.material.color,Color.black,(Time.time - startTime)/destructionTime);
        } */
    }
}
