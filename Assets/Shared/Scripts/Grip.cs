using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic
{
    public class GripInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 left
        {
            get
            {
                return Vector3.Normalize( rotation * Vector3.left);
            }
        }
        public Vector3 right
        {
            get
            {
                return Vector3.Normalize(rotation * Vector3.right);
            }
        }
        public Vector3 up
        {
            get
            {
                return Vector3.Normalize(rotation * Vector3.up);
            }
        }
        public Vector3 down
        {
            get
            {
                return Vector3.Normalize(rotation * Vector3.down);
            }
        }
        public Vector3 normal
        {
            get
            {
                return Vector3.Normalize(rotation * Vector3.back);
            }
        }
        public GripInfo(Vector3 pos, Vector3 n)
        {
            position = pos;
            rotation = Quaternion.LookRotation(-n, Vector3.up);
        }
    }

    public class Grip : MonoBehaviour
    {
        public static List<Grip> list = new List<Grip>();

        // Use this for initialization
        void Start()
        {
            if (!list.Contains(this)) list.Add(this);
            OnStart();
        }

        // Update is called once per frame
        void Update()
        {
            OnUpdate();
        }

        public virtual void OnStart()
        {

        }
        public virtual void OnUpdate()
        {

        }

        public virtual List<GripInfo> GetPoints(Vector3 input_dir, Vector3 base_pos, float reach)
        {
            return new List<GripInfo>();
        }

        public static List<GripInfo> GetAllPoints(Vector3 input_dir, Vector3 base_pos, float reach)
        {
            List<GripInfo> l = new List<GripInfo>();
            foreach (Grip i in list)
            {
                l.AddRange(i.GetPoints(input_dir, base_pos, reach));
            }
            return l;
        }
    }
}