using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    Vector3 velocity = Vector3.zero;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    
        transform.position += velocity * Time.deltaTime;
        velocity.y -= 9.81f * Time.deltaTime;

        
    }
}
