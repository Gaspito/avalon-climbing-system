using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Logic
{
    [CustomEditor(typeof(Mesh_Grip))]
    public class Mesh_GripEditor : Editor
    {
        /*
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawGizmosNonSelected(Transform obj, GizmoType gizmoType)
        {
            DrawGizmos(obj, gizmoType);
        }
        */

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmosSelected(Transform obj, GizmoType gizmoType)
        {
            DrawGizmos(obj, gizmoType);
        }

        static void DrawGizmos(Transform obj, GizmoType gizmoType)
        {
            Mesh_Grip g = obj.GetComponent<Mesh_Grip>();
            if (g != null)
            {
                if (!g.IsInitialized()) g.OnStart();
                SceneView view = SceneView.currentDrawingSceneView;
                if (view == null) return;
                Vector3 hit = g.AABB.ClosestPoint(view.camera.transform.position);
                Vector3 to_obj = hit - view.camera.transform.position;
                float dist = to_obj.magnitude;
                if (dist > 10) return;
                foreach (MeshGripEdge edge in g.edges)
                {
                    Handles.color = Color.cyan;
                    Handles.DrawDottedLine(edge.start, edge.end, 2);
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
                    Handles.color = Color.cyan;
                    Handles.DrawLine(edge.start, edge.end);
                    Vector3 hw = (edge.start + edge.end) / 2.0f;
                    Handles.color = new Color(0, 1, 1, 0.25f);
                    Handles.ArrowHandleCap(0, hw, Quaternion.LookRotation(edge.normal), 0.5f, EventType.Repaint);
                }
                foreach (MeshGripPoint point in g.points)
                {
                    Handles.color = new Color(0, 1, 1, 0.25f);
                    Handles.SphereHandleCap(1, point.point, obj.rotation, 0.1f, EventType.Repaint);
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
                    Handles.color = Color.cyan;
                    Handles.SphereHandleCap(1, point.point, obj.rotation, 0.1f, EventType.Repaint);
                    Handles.color = new Color(0, 1, 1, 0.25f);
                    Handles.ArrowHandleCap(0, point.point, Quaternion.LookRotation(point.normal), 0.5f, EventType.Repaint);
                }
            }
        }
    }
}