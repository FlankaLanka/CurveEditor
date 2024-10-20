using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadCreator : MonoBehaviour
{
    [Range(0.05f, 1.5f)]
    public float spacing = 1;
    [Range(0.001f,5f)]
    public float roadWidth = 1f;
    public bool autoUpdate = true;

    public void UpdateRoad()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector2[] points = path.CalculateEvenlySpacedPoints(spacing);
        GetComponent<MeshFilter>().mesh = CreateRoadMesh(points, path.IsClosed);
    }

    public Mesh CreateRoadMesh(Vector2[] points, bool isClosed)
    {
        if (points.Length < 2)
            return null;

        Vector3[] verts = new Vector3[points.Length * 2];
        int isClosedExtraTriangles = isClosed ? 2 : 0;
        int[] tris = new int[(2 * points.Length - 2) * 3 + isClosedExtraTriangles * 3];
        Vector2[] uvs = new Vector2[verts.Length];

        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 dir;
            if (i == 0)
                dir = points[i + 1] - points[i];
            else if (i == points.Length - 1)
                dir = points[i] - points[i - 1];
            else
                dir = (points[i + 1] - points[i]) + (points[i] - points[i - 1]);

            dir.Normalize();
            Vector2 left = new Vector2(-dir.y, dir.x);

            //set vertices
            verts[vertIndex] = points[i] + left * roadWidth / 2; //0, 2 ,4...
            verts[vertIndex + 1] = points[i] - left * roadWidth / 2; //1, 3, 5...

            float completePercent = i / (float)(points.Length - 1);
            float mountainUV = Mathf.Abs(Mathf.Abs(completePercent - 0.5f) - 0.5f);
            uvs[vertIndex] = new Vector2(1, mountainUV);
            uvs[vertIndex + 1] = new Vector2(0, mountainUV);


            //set triangles
            if (i < points.Length - 1)
            {
                //clockwise direction starting from curIndex
                //this also sets triangles that uses vertices that haven't been set (for example, at 0, already sets triangles that uses vertices at 1

                tris[triIndex++] = vertIndex;
                tris[triIndex++] = vertIndex + 2;
                tris[triIndex++] = vertIndex + 1;

                tris[triIndex++] = vertIndex + 1;
                tris[triIndex++] = vertIndex + 2;
                tris[triIndex++] = vertIndex + 3;
            }

            vertIndex += 2;
        }

        //if is closed, modify first and last points vertex, then draw 2 more triangles
        if(isClosed)
        {
            Vector2 newDir = (points[1] - points[0]) + (points[0] - points[points.Length - 1]);
            newDir.Normalize();
            Vector2 newLeft = new Vector2(-newDir.y, newDir.x);
            verts[0] = points[0] + newLeft * roadWidth / 2;
            verts[1] = points[0] - newLeft * roadWidth / 2;

            newDir = (points[0] - points[points.Length - 1]) + (points[points.Length - 1] - points[points.Length - 2]);
            newDir.Normalize();
            newLeft = new Vector2(-newDir.y, newDir.x);
            verts[verts.Length - 2] = points[points.Length - 1] + newLeft * roadWidth / 2;
            verts[verts.Length - 1] = points[points.Length - 1] - newLeft * roadWidth / 2;

            tris[triIndex++] = verts.Length - 2;
            tris[triIndex++] = 0;
            tris[triIndex++] = verts.Length - 1;

            tris[triIndex++] = verts.Length - 1;
            tris[triIndex++] = 0;
            tris[triIndex++] = 1;
        }


        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }
}
