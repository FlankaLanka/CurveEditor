using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlacer : MonoBehaviour
{
    public Path path;
    public float spacing = 0.1f;
    public float resolution = 1f;

    List<GameObject> oldPoints = new List<GameObject>();
    Vector2[] points;

    private void Start()
    {
        if (path == null)
            path = FindAnyObjectByType<PathCreator>().path;
    }

    int i = 0;
    void Update()
    {
        i++;
        if (resolution < 0 || i < 40)
            return;
        i = 0;

        foreach (GameObject p in oldPoints)
        {
            Destroy(p);
        }

        points = path.CalculateEvenlySpacedPoints(spacing, resolution);
        foreach (Vector2 p in points)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = p;
            g.transform.localScale = Vector3.one * spacing / 2;
            oldPoints.Add(g);
        }
    }
}
