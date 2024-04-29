using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Logic
{
    [CustomEditor(typeof(Line_Grip))]
    public class Line_GripEditor : Editor
    {
        private void OnSceneGUI()
        {
            Line_Grip g = (Line_Grip)target;
            if (g._start_transform == null) g._start_transform = g.transform;
            if (g._end_transform == null)
            {
                GameObject obj = new GameObject(g.name + " End");
                obj.transform.position = g._start + g._start_transform.forward;
                obj.transform.rotation = g._start_transform.rotation;
                obj.transform.SetParent(g._start_transform, true);
                g._end_transform = obj.transform;
            }
            g._end_transform.position = Handles.PositionHandle(g._end, g._start_transform.rotation);
        }

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
            Line_Grip g = obj.GetComponent<Line_Grip>();
            if (g != null)
            {
                if (g._start_transform != null && g._end_transform != null)
                {
                    Handles.color = Color.red;
                    Handles.DrawDottedLine(g._start, g._end, 2);
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
                    Handles.color = Color.cyan;
                    Handles.DrawLine(g._start, g._end);
                    Vector3 hw = (g._start + g._end) / 2.0f;
                    Handles.color = new Color(0, 1, 1, 0.25f);
                    Handles.ArrowHandleCap(0, hw, Quaternion.LookRotation(obj.TransformDirection(Vector3.left)), 0.5f, EventType.Repaint);
                }
            }
        }
    }

    
}