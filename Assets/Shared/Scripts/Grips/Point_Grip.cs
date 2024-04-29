using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic
{
    public class Point_Grip : Grip
    {
        public Vector3 position
        {
            get
            {
                return transform.position;
            }
        }

        public override List<GripInfo> GetPoints(Vector3 input_dir, Vector3 base_pos, float reach)
        {
            if (Vector3.Distance(position, base_pos)<=reach)
                return new List<GripInfo>() { new GripInfo(position, transform.TransformDirection(Vector3.left))};
            return new List<GripInfo>();
        }
    }
}