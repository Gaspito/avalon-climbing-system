using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IK))]
public class IKEditor : Editor {

    public override void OnInspectorGUI()
    {
        IK ik = (IK)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Reset"))
        {
            ik.ForceInit();
        }
    }

    

    private void OnSceneGUI()
    {
        IK ik = (IK)target;
        Handles.color = Color.white;
        if (ik._target != null)
        {
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, ik._target, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.color = Color.white;
            ik._target_transform.position = Handles.PositionHandle(ik._target, Quaternion.identity);
            Handles.Label(ik._target, new GUIContent("Target"));
        }
        if (ik._axis != null)
        {
            Handles.color = Color.blue;
            if (ik._target != null)
            {
                Handles.DrawLine(ik._target, ik._axis.position);
                Handles.DrawLine(ik.transform.position, ik._axis.position);
            }
            Handles.SphereHandleCap(0, ik._axis.position, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.color = Color.white;
            ik._axis.position = Handles.PositionHandle(ik._axis.position, Quaternion.identity);
            Handles.Label(ik._axis.position, new GUIContent("Axis"));
        }
        
        if (ik._transforms.Count >= 2)
        {
            if (ik.GetBones() == null) ik.ForceInit();
            if (ik.GetBones().Count == 0) ik.ForceInit();

            Handles.color = Color.red;
            Handles.DrawLine(ik.GetBones()[0]._head, ik._target);

            for (int i = 0; i < ik.GetBones().Count; i++)
            {
                IK_Bone b = ik.GetBones()[i];
                Handles.color = Color.yellow;
                Handles.DrawDottedLine(b._head_transform.position, b._tail_transform.position, 3);
                Quaternion roll = Quaternion.LookRotation(b._tail - b._head);
                Handles.ArrowHandleCap(i, b._head, roll, b._length, EventType.Repaint);
                Handles.Label((b._head + b._tail) * 0.5f, new GUIContent("Bone " + i + ", length: " + b._length));
            }
        }
        
    }
}
