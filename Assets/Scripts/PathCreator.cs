using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    //[HideInInspector]
    public Path path;

    public Color anchorColor = Color.red;
    public Color controlColor = Color.white;
    public Color handleColor = Color.green;
    public Color segmentColor = Color.magenta;
    public Color selectedSegmentColor = Color.gray;

    public void CreatePath()
    {
        path = new Path(transform.position);
    }
}
