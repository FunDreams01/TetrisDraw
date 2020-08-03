using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRandom : MonoBehaviour
{
    public Texture[] BlockTextures;
    public int Index;
    public void Init(int Index = -1) // I call init myself when needed. It is useful for starting functions that require additional input data from other scripts.
    {
        if(Index == -1) Index = Random.Range(0,BlockTextures.Length);
        Texture tex = BlockTextures[Index];
        GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", tex);
    }
}
