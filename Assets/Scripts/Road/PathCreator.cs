using UnityEngine;

public class PathCreator : MonoBehaviour
{

    [HideInInspector]
    public Path path;

    public Color anchorCol = Color.red;
    public Color controlCol = Color.white;
    public Color segmentCol = Color.green;
    public Color selectedSegmentCol = Color.yellow;
    public float anchorDiameter = .1f;
    public float controlDiameter = .075f;
    public bool displayControlPoints = true;

    public void CreatePath()
    {
        path = new Path(transform.position);
        RoadCreator road = GetComponent<RoadCreator>();
        if (road != null)
        {
            road.DestroyWalls();
            road.UpdateRoad();
            road.UpdateRoad();
        }
    }

    void Reset()
    {
        CreatePath();
    }
}
