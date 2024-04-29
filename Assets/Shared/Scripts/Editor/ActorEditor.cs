using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Logic
{
    [CustomEditor(typeof(Actor), true)]
    public class ActorEditor : Editor
    {
        

        [DrawGizmo(GizmoType.NonSelected)]
        static void RenderCustomGizmo(Transform obj, GizmoType gizmoType)
        {
            DrawGizmos(obj, gizmoType);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void RenderCustomGizmoSelected(Transform obj, GizmoType gizmoType)
        {
            DrawGizmos(obj, gizmoType);
        }

        static void DrawGizmos(Transform obj, GizmoType gizmoType)
        {
            Actor a = obj.GetComponent<Actor>();
            if (a != null && obj != null)
            {

                SceneView view = SceneView.currentDrawingSceneView;
                if (view == null) return;
                Vector3 to_obj = obj.position - view.camera.transform.position;
                float dist = to_obj.magnitude;
                float angle = Vector3.Angle(to_obj, view.camera.transform.forward);
                if (dist < a.GetEditorDrawDistance() && angle < 90)
                {
                    Handles.CircleHandleCap(0, obj.position, Quaternion.LookRotation(Vector3.up), 0.5f, EventType.Repaint);
                    Handles.ArrowHandleCap(1, obj.position + obj.forward * 0.5f, obj.rotation, 0.5f, EventType.Repaint);

                    Vector3 screen_pos = view.camera.WorldToScreenPoint((obj.position + Vector3.up * 2.2f));
                    screen_pos.y = Handles.GetMainGameViewSize().y - screen_pos.y;
                    Rect r = new Rect(screen_pos.x - 100, screen_pos.y, 200, 200);

                    Handles.BeginGUI();
                    GUI.BeginGroup(r);

                    GUI.Label(new Rect(25, 0, 150, 25), a.GetName(), Actor.GetSkin().label);

                    GUI.EndGroup();
                    Handles.EndGUI();

                }
                else
                {
                    Gizmos.DrawIcon(obj.position + Vector3.up, "ICO Actor 0");
                }

                //Handles.Label(obj.position + Vector3.up, new GUIContent(a.GetName()), Actor.GetSkin().label);

            }
        }
    }
}