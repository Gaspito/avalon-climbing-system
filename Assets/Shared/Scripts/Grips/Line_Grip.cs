using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic
{
    public class Line_Grip : Grip
    {
        public Transform _start_transform;
        public Transform _end_transform;

        public Vector3 _start
        {
            get
            {
                return _start_transform.position;
            }
        }
        public Vector3 _end
        {
            get
            {
                return _end_transform.position;
            }
        }

        public override List<GripInfo> GetPoints(Vector3 input_dir, Vector3 base_pos, float reach)
        {
            List<GripInfo> output = new List<GripInfo>();
            Vector3 sp = base_pos - _start;
            Vector3 se = _end - _start;
            Vector3 sq = Vector3.Project(sp, se);
            Vector3 q = _start + sq;
            Vector3 pq = q - base_pos;
            if (pq.magnitude == reach)
            {
                output.Add(new GripInfo(q, _start_transform.TransformDirection(Vector3.left)));
                return output;
            } else if (pq.magnitude > reach)
            {
                return output;
            }
            float angle = Mathf.Acos(pq.magnitude / reach);
            float aq_length = Mathf.Sin(angle) * reach;
            Vector3 a = q + sq.normalized * aq_length;
            Vector3 b = q - sq.normalized * aq_length;

            Vector3 qa = a - q;
            Vector3 qb = b - q;
            Vector3 sa = a - _start;
            Vector3 sb = b - _start;
            Vector3 ea = a - _end;
            Vector3 eb = b - _end;
            Vector3 eq = q - _end;

            if (sa.magnitude < se.magnitude && ea.magnitude < se.magnitude)
                output.Add(new GripInfo(a, _start_transform.TransformDirection(Vector3.left)));
            if (sb.magnitude < se.magnitude && eb.magnitude < se.magnitude)
                output.Add(new GripInfo(b, _start_transform.TransformDirection(Vector3.left)));
            if (sq.magnitude < se.magnitude && eq.magnitude < se.magnitude)
                output.Add(new GripInfo(q, _start_transform.TransformDirection(Vector3.left)));
            return output;
        }
    }
}