using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Logic
{
    [CustomEditor(typeof(Point_Grip))]
    public class Point_GripEditor : Editor
    {

        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawGizmosNonSelected(Transform obj, GizmoType gizmoType)
        {
            if (show_in_editor) DrawGizmos(obj, gizmoType);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmosSelected(Transform obj, GizmoType gizmoType)
        {
            DrawGizmos(obj, gizmoType);
        }

        static bool show_in_editor = true;

        static void DrawGizmos(Transform obj, GizmoType gizmoType)
        {
            Point_Grip g = obj.GetComponent<Point_Grip>();
            if (g != null)
            {
                Handles.color = new Color(1, 0, 0, 0.25f);
                Handles.SphereHandleCap(1, g.position, obj.rotation, 0.1f, EventType.Repaint);
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
                Handles.color = Color.cyan;
                Handles.SphereHandleCap(1, g.position, obj.rotation, 0.1f, EventType.Repaint);
                Handles.color = new Color(0, 1, 1, 0.25f);
                Handles.ArrowHandleCap(0, g.position, Quaternion.LookRotation(obj.TransformDirection(Vector3.left)), 0.5f, EventType.Repaint);
            }
        }

    }
}