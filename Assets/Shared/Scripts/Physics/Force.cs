using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.PhysicsEngine
{
    // #################################################################
    //
    // Classical Mechanic forces from physics class
    // All are utility classes.
    //
    // #################################################################


    [System.Serializable]
    public class Force
    {
        public virtual Vector3 vector
        {
            get
            {
                return Vector3.zero;
            }
        }
        public virtual float intensity
        {
            get
            {
                return vector.magnitude;
            }
        }
        public virtual Vector3 direction
        {
            get
            {
                return vector.normalized;
            }
        }
    }
    [System.Serializable]
    public class ResultForce : Force
    {
        public List<Force> forces;
        public ResultForce()
        {
            forces = new List<Force>();
        }
        public void Add(Force f)
        {
            forces.Add(f);
        }
        public void Remove(Force f)
        {
            forces.Remove(f);
        }
        public void RemoveOfType<T>()
        {
            List<Force> to_remove = new List<Force>();
            foreach (Force f in forces)
            {
                if (f.GetType() == typeof(T) || f.GetType().IsSubclassOf(typeof(T)))
                {
                    to_remove.Add(f);
                }
            }
            foreach (Force f in to_remove)
            {
                forces.Remove(f);
            }
        }
        public T GetType<T>() where T:class
        {
            foreach (Force f in forces)
            {
                if (f.GetType() == typeof(T) || f.GetType().IsSubclassOf(typeof(T)))
                {
                    return f as T;
                }
            }
            return null;
        }
        public override Vector3 vector
        {
            get
            {
                Vector3 f = Vector3.zero;
                foreach (Force i in forces)
                {
                    f += i.vector;
                }
                return f;
            }
        }
    }
    [System.Serializable]
    public class Force_Gravity : Force
    {
        float m;
        public static float g = 9.8f;
        public Force_Gravity(float mass)
        {
            m = mass;
        }
        public override Vector3 vector
        {
            get
            {
                return Vector3.down * m * g;
            }
        }
    }
    [System.Serializable]
    public class Force_Normal : Force
    {
        float m;
        Vector3 _normal;
        public Force_Normal(float mass, Vector3 normal)
        {
            m = mass;
            _normal = normal.normalized;
        }
        public override Vector3 vector
        {
            get
            {
                float angle = Vector3.Angle(_normal, Vector3.up);
                float normal_down = Mathf.Cos(angle) * m * Force_Gravity.g;
                return _normal.normalized * normal_down;
            }
        }
    }
    [System.Serializable]
    public class Force_Motion : Force
    {
        public Vector3 motion;
        public Force_Motion(Vector3 f)
        {
            motion = f;
        }
        public override Vector3 vector
        {
            get
            {
                return motion;
            }
        }
    }
}