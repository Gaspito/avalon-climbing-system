using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic
{
    public class Surface_Grip : Grip
    {

        public Transform _Top_Left;
        public Transform _Top_Right;
        public Transform _Bottom_Left;
        public Transform _Bottom_Right;

        public Vector3 a
        {
            get
            {
                return _Bottom_Left.position;
            }
        }
        public Vector3 b
        {
            get
            {
                return _Top_Left.position;
            }
        }
        public Vector3 c
        {
            get
            {
                return _Top_Right.position;
            }
        }
        public Vector3 d
        {
            get
            {
                return _Bottom_Right.position;
            }
        }

        public int _point_count = 16;

        public bool IsPointOnPlane(Vector3 p)
        {
            float angle_a = Vector3.Angle(b - a, p - a);
            float angle_b = Vector3.Angle(d - a, p - a);
            if (angle_a > 90 || angle_b > 90) return false;
            return true;
        }

        public override List<GripInfo> GetPoints(Vector3 input_dir, Vector3 base_pos, float reach)
        {
            List<GripInfo> output = new List<GripInfo>();
            Plane p = new Plane(a, b, c);
            Vector3 q = p.ClosestPointOnPlane(base_pos);
            Vector3 pq = q - base_pos;
            if (pq.magnitude > reach) return output;
            else if (pq.magnitude == reach)
            {
                output.Add(new GripInfo(q, p.normal));
                return output;
            }
            float angle = Mathf.Acos(pq.magnitude / reach);
            float r = Mathf.Sin(angle) * reach;
            float delta_i = Mathf.PI / (float)_point_count;
            Vector3 ab = b - a;
            Vector3 ad = d - a;
            for (int i = 0; i < _point_count; i++)
            {
                Vector3 v = ab.normalized * Mathf.Cos(delta_i * i) + ad.normalized * Mathf.Sin(delta_i * i);
                Vector3 intersection = q + v.normalized * r;
                if (IsPointOnPlane(intersection))
                {
                    output.Add(new GripInfo(intersection, p.normal));
                }
            }
            return output;
        }
    }
}