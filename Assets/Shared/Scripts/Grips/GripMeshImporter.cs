using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Text;

/// <summary>
/// Handles .grip meshes exported from blender.
/// </summary>
[ScriptedImporter(1, ".grip")]
public class GripMeshImporter : ScriptedImporter {

    public override void OnImportAsset(AssetImportContext ctx)
    {
        GripMesh mesh = new GripMesh();
        ctx.AddObjectToAsset("MainAsset", mesh);

        List<Vector3> vertices = new List<Vector3>();
        List<int> edges = new List<int>();
        List<int> tris = new List<int>();

        string text = File.ReadAllText(ctx.assetPath, Encoding.UTF8);
        string[] lines = text.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        for (int lid =0; lid < lines.Length; lid++)
        {
            string line = lines[lid];
            if (line.StartsWith("v")) // vertex
            {
                string[] v = line.Split(new string[] { " " }, System.StringSplitOptions.None);
                if (v.Length > 3)
                {
                    float x = float.Parse(v[1]);
                    float y = float.Parse(v[2]);
                    float z = float.Parse(v[3]);
                    vertices.Add(new Vector3(x, y, z));
                }
            } else if (line.StartsWith("l"))
            {
                string[] l = line.Split(new string[] { " " }, System.StringSplitOptions.None);
                if (l.Length > 2)
                {
                    int e1 = int.Parse(l[1]);
                    int e2 = int.Parse(l[2]);
                    edges.Add(e1);
                    edges.Add(e2);
                }
            }
        }
    }

}
