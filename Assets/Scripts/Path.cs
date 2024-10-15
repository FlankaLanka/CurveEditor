using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Path
{
    [SerializeField]
    private List<Vector2> points;
    [SerializeField]
    private bool isClosed = false;
    [SerializeField]
    private bool autoSetControlPoints;

    public Path(Vector2 center)
    {
        points = new List<Vector2>
        {
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) / 2f,
            center + (Vector2.right + Vector2.down) / 2f,
            center + Vector2.right
        };
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public int NumPoints
    {
        get
        {
            return points.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            return points.Count / 3;
        }
    }

    public bool AutoSetControlPoints
    {
        get
        {
            return autoSetControlPoints;
        }
        set
        {
            if(autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                if (autoSetControlPoints)
                    AutoSetAllControlPoints();
            }

        }
    }
    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
    }

    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        return null;
    }

    public void AddSegment(Vector2 anchorPos)
    {
        if (points.Count < 2)
            return;

        Vector2 endingControlPoint = (points[points.Count - 1] - points[points.Count - 2]) + points[points.Count - 1];
        points.Add(endingControlPoint);
        points.Add((endingControlPoint + anchorPos) / 2);
        points.Add(anchorPos);

        if(autoSetControlPoints)
            AutoSetAllAffectedControlPoints(points.Count - 1);
    }

    public void SplitSegment(Vector2 anchorPos, int segmentIndex)
    {
        Vector2[] temp = new Vector2[] { Vector2.zero, anchorPos, Vector2.zero };
        points.InsertRange(segmentIndex * 3 + 2, temp);
        if (autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
        }
        else
        {
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
        }
    }

    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments <= 1 || (NumSegments <= 2 && isClosed))
            return;

        if(anchorIndex == 0)
        {
            //if closed change last point to 2
            if (isClosed)
                points[points.Count - 2] = points[2];
            //remove 0,1,2
            points.RemoveRange(0, 3);

        }
        else if(anchorIndex == points.Count - 1 && !isClosed)
        {
            //remove last,last-1,last-2
            points.RemoveRange(anchorIndex - 2, 3);
        }
        else if(anchorIndex == points.Count - 2 && isClosed)
        {
            //math works out to be same as default
            points.RemoveRange(anchorIndex - 1, 3);
        }
        else
        {
            //remove i-1,i,i+1
            points.RemoveRange(anchorIndex - 1, 3);
        }
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new Vector2[]
        {
            points[i * 3], //anchor
            points[i * 3 + 1], //control
            points[i * 3 + 2], //control
            points[LoopIndex(i * 3 + 3)], //anchor
        };
    }

    public void MovePoint(int i, Vector2 pos)
    {
        Vector2 deltaMove = pos - points[i];

        if(IsAnchor(i) || !autoSetControlPoints)
            points[i] = pos;

        if(autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(i);
            return;
        }


        if(IsAnchor(i)) //anchor
        {
            if(i > 0 || isClosed)
                points[LoopIndex(i - 1)] += deltaMove;
            if (i < points.Count - 1 || isClosed)
                points[LoopIndex(i + 1)] += deltaMove;
        }
        else //control
        {
            bool nextIsAnchor = IsAnchor(i + 1);
            int correspondingControl = nextIsAnchor ? i + 2 : i - 2;
            int correspondingAnchor = nextIsAnchor ? i + 1 : i - 1;
            if(correspondingControl > 0 && correspondingControl < points.Count || isClosed)
            {
                if (isClosed)
                {
                    correspondingAnchor = LoopIndex(correspondingAnchor);
                    correspondingControl = LoopIndex(correspondingControl);
                }

                float currentLength = Vector2.Distance(points[correspondingAnchor], points[correspondingControl]);
                Vector2 newDirection = (points[correspondingAnchor] - points[i]).normalized;
                points[correspondingControl] = (currentLength * newDirection) + points[correspondingAnchor];
            }

        }
    }

    public void ToggleClosed()
    {
        isClosed = !isClosed;

        if(isClosed)
        {
            Vector2 endingControlPoint = (points[points.Count - 1] - points[points.Count - 2]) + points[points.Count - 1];
            points.Add(endingControlPoint);

            Vector2 startingControlPoint = (points[0] - points[1]) + points[0];
            points.Add(startingControlPoint);

            if(autoSetControlPoints)
            {
                AutoSetAnchorControlPoints(0);
                AutoSetAnchorControlPoints(points.Count - 3);
            }
        }
        else
        {
            points.RemoveRange(points.Count - 2, 2);
            if(autoSetControlPoints)
            {
                AutoSetStartAndEndControls();
            }
        }
    }

    //private void AutoSetAnchorControlPoints(int anchorIndex)
    //{
    //    Vector2 curAnchor = points[anchorIndex];
    //    Vector2[] dir = new Vector2[2];
    //    float[] lengths = new float[2];

    //    if(anchorIndex - 3 >= 0 || isClosed)
    //    {
    //        Vector2 prevAnchor = points[LoopIndex(anchorIndex - 3)];
    //        dir[0] = (prevAnchor - curAnchor).normalized;
    //        lengths[0] = Vector2.Distance(curAnchor, prevAnchor);
    //    }

    //    if (anchorIndex + 3 <= points.Count - 1 || isClosed)
    //    {
    //        Vector2 nextAnchor = points[LoopIndex(anchorIndex + 3)];
    //        dir[0] = (nextAnchor - curAnchor).normalized;
    //        lengths[0] = Vector2.Distance(curAnchor, nextAnchor);
    //    }
    //}

    void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if (i >= 0 && i < points.Count || isClosed)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }

        AutoSetStartAndEndControls();
    }

    void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; i += 3)
        {
            AutoSetAnchorControlPoints(i);
        }

        AutoSetStartAndEndControls();
    }

    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPos = points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }
        if (anchorIndex + 3 >= 0 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
            {
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * .5f;
            }
        }
    }

    void AutoSetStartAndEndControls()
    {
        if (!isClosed)
        {
            points[1] = (points[0] + points[2]) * .5f;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * .5f;
        }
    }


    private bool IsAnchor(int i)
    {
        return i % 3 == 0;
    }

    private int AnchorIndex(int i)
    {
        if (!IsAnchor(i))
            return -1;

        return i / 3 + 1;
    }

    private int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }

}
