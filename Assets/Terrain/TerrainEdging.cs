using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainEdging : MonoBehaviour
{
    public Transform edgeX1;
    public Transform edgeZ1;
    public Transform edgeX2;
    public Transform edgeZ2;

    public Transform edgeLeftWing;
    public Transform edgeMiddleWing;
    public Transform edgeRightWing;

    void Start()
    {
        
    }

    public void updateEdges(float mapSize)
    {
        if (edgeX1 != null)
        {
            float halfMapSize = mapSize / 2f;

            //Update edges position & scale
            edgeX1.localPosition = new Vector3(halfMapSize / 200f, 0, halfMapSize / 200f);
            edgeZ1.localPosition = new Vector3(halfMapSize / 200f, 0, halfMapSize / 200f);
            edgeX2.localPosition = new Vector3(halfMapSize / 200f, 0, halfMapSize / 200f * 3f);
            edgeZ2.localPosition = new Vector3(halfMapSize / 200f * 3f, 0, halfMapSize / 200f);

            edgeX1.localScale = new Vector3(55, 1, halfMapSize);
            edgeZ1.localScale = new Vector3(55, 1, halfMapSize);
            edgeX2.localScale = new Vector3(55, 1, halfMapSize);
            edgeZ2.localScale = new Vector3(55, 1, halfMapSize);
        }

        //Update edge wings
        if (edgeLeftWing != null)
        {
            float x = mapSize / 64f;
            float z = mapSize / 64f;

            edgeLeftWing.localPosition = new Vector3(0, 0, z);
            edgeMiddleWing.localPosition = new Vector3(x, 0, z);
            edgeRightWing.localPosition = new Vector3(x, 0, 0);
        }
    }

}
