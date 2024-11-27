using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTravelPath : MonoBehaviour
{
    public PathCreator creator;
    public RoadCreator road;
    public float speed = 1f;

    private Vector2[] points;
    private int currentPointIndex = 0;
    private bool endReachedInOpenPath = false;

    // Start is called before the first frame update
    void Start()
    {
        points = creator.path.CalculateEvenlySpacedPoints(road.spacing);
    }

    // Update is called once per frame
    void Update()
    {
        if (points == null || points.Length < 2 || endReachedInOpenPath == true)
        {
            return;
        }

        Vector2 start = points[currentPointIndex % points.Length];
        Vector2 end = points[(currentPointIndex + 1) % points.Length];

        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(end, start);

        Vector2 endPosition = new Vector2(transform.position.x, transform.position.y) + (direction * Time.deltaTime * speed);
        transform.position = endPosition;

        float angle = Vector2.SignedAngle(transform.up, direction);
        transform.Rotate(Vector3.forward, angle);

        if (distance < Vector2.Distance(endPosition, start)) //overshot, move to next indices
        {
            transform.position = end;
            currentPointIndex += 1;

            if (currentPointIndex >= points.Length - 1 && !creator.path.IsClosed)
            {
                endReachedInOpenPath = true;
                return;
            }
            currentPointIndex %= points.Length;
        }
    }
}
