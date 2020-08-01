using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRandom : MonoBehaviour
{
    public Texture[] BlockTextures;
    void Start()
    {
        this.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", BlockTextures[Random.Range(0, BlockTextures.Length)]);
    }


}
