using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadCreator : MonoBehaviour
{

    [Range(.05f, 1.5f)]
    public float spacing = 1;
    public float roadWidth = 1;
    [HideInInspector]
    public bool autoUpdate = false;
    public float tiling = 1;
    [HideInInspector]
    public bool generateWalls = true;
    [HideInInspector]
    public bool showWalls = true;
    
    public float wallsWidth = 0.5f;
    public float wallsHeight = 0.5f;

    private GameObject walls = null;

    private void Start()
    {
        SetupPath();
    }

    public void SetupPath()
    {
        GetComponent<PathCreator>().path.Mode3D = true;
    }

    public void UpdateRoad()
    {
        SetupPath();
        Path path = GetComponent<PathCreator>().path;
        Vector3[] points = path.CalculateEvenlySpacedPoints(spacing);


        GetComponent<MeshFilter>().mesh = CreateRoadMesh(points, path.IsClosed);
        int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * .05f);
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
    }

    Mesh CreateRoadMesh(Vector3[] points, bool isClosed)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }
            if (i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }

            forward.Normalize();
            Vector3 normal = new Vector3(-forward.z, 0, forward.x);

            verts[vertIndex] = points[i] + normal * roadWidth / 2f;
            verts[vertIndex + 1] = points[i] - normal * roadWidth / 2f;

            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector3(0, v);
            uvs[vertIndex + 1] = new Vector3(1, v);

            if (i < points.Length - 1 || isClosed)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        if (generateWalls)
        {
            DestroyWalls();
            CreateWalls(verts, isClosed);
        }

        return mesh;
    }

    void CreateWalls(Vector3[] points, bool isClosed)
    {
        GameObject leftWall = Instantiate(gameObject);
        leftWall.transform.position = Vector3.zero;
        leftWall.transform.rotation = Quaternion.identity;
        leftWall.name = "Left Walls";

        GameObject rightWall = Instantiate(gameObject);
        rightWall.transform.position = Vector3.zero;
        rightWall.transform.rotation = Quaternion.identity;
        rightWall.name = "Right Walls";

        walls = Instantiate(gameObject);
        walls.transform.position = Vector3.zero;
        walls.transform.rotation = Quaternion.identity;
        walls.name = "Walls";

        for (int i = 0; i < points.Length - 2; i += 2)
        {
            CreateWall(points[i], points[i + 2], 1, leftWall.transform);
            CreateWall(points[i + 1], points[i + 3], -1, rightWall.transform);
        }
        if (isClosed)
        {
            CreateWall(points[points.Length - 2], points[0], 1, rightWall.transform);
            CreateWall(points[points.Length - 1], points[1], -1, leftWall.transform);
        }
        leftWall.transform.parent = walls.transform;
        rightWall.transform.parent = walls.transform;
        walls.transform.parent = transform;
    }

    void CreateWall(Vector3 a, Vector3 b, float dir, Transform parent)
    {
        float tmpWallsWidth = wallsWidth / 2;
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 forward = b - a;
        Vector3 normal = new Vector3(-forward.z, 0, forward.x);
        wall.transform.position = a + forward / 2 + dir * normal * tmpWallsWidth;
        wall.transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        wall.transform.localScale = new Vector3(wallsWidth, wallsHeight, forward.magnitude);
        wall.GetComponent<MeshRenderer>().enabled = showWalls;
        wall.transform.parent = parent;
    }

    public void ResetWalls(bool newGen)
    {
        if (generateWalls && newGen)
        {
            SetupPath();
            UpdateRoad();
        }
        else if(!generateWalls)
        {
            DestroyWalls();
        }
        if (walls != null)
        {
            GameObject left = walls.transform.GetChild(0).gameObject;
            GameObject right = walls.transform.GetChild(1).gameObject;
            for(int i = 0; i<left.transform.childCount; i++)
            {
                left.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = showWalls;
            }
            for (int i = 0; i < right.transform.childCount; i++)
            {
                right.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = showWalls;
            }
        }
    }

    public void DestroyWalls() {
        if (walls != null)
        {
            DestroyImmediate(walls);
        }
        walls = null;
    }

}
