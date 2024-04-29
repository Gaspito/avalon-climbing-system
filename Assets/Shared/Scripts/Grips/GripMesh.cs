using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a cluster of points, edges, and triangles
/// that the player can climb on.
/// A grip mesh is generated via the GripMeshImporter, which takes .grip meshes and converts them.
/// </summary>
public class GripMesh : ScriptableObject {

    public List<Vector3> point_grips;
    public List<Vector3> point_grips_normal;
    public List<Vector3> edge_grips_start;
    public List<Vector3> edge_grips_end;
    public List<Vector3> edge_grips_normal;
    public List<Vector3> tris_grips_a;
    public List<Vector3> tris_grips_b;
    public List<Vector3> tris_grips_c;
    public List<Vector3> tris_grips_normal;

    public GripMesh()
    {
        point_grips = new List<Vector3>();
        point_grips_normal = new List<Vector3>();
        edge_grips_start = new List<Vector3>();
        edge_grips_end = new List<Vector3>();
        edge_grips_normal = new List<Vector3>();
        tris_grips_a = new List<Vector3>();
        tris_grips_b = new List<Vector3>();
        tris_grips_c = new List<Vector3>();
        tris_grips_normal = new List<Vector3>();
    }

}
