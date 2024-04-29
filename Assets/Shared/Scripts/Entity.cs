using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Logic.Weapons;

namespace Game.Logic
{
    public class Entity : MonoBehaviour
    {
        public float health;
        public virtual float max_health
        {
            get
            {
                return 100;
            }
        }
        public virtual bool dead
        {
            get
            {
                return (health <= 0);
            }
        }
        public virtual Vector3 position
        {
            get
            {
                return transform.position;
            }set
            {
                transform.position = value;
            }
        }
        public virtual Vector3 direction
        {
            get
            {
                return transform.TransformDirection(Vector3.forward);
            }
            set
            {
                transform.rotation = Quaternion.LookRotation(value, Vector3.up);
            }
        }
        public virtual void Move(Vector3 direction, float speed)
        {
            position += direction.normalized*speed*Time.deltaTime;
        }

        public Weapon weapon;

        public static List<Entity> entities = new List<Entity>();

        // Use this for initialization
        void Start()
        {
            entities.Add(this);
            health = max_health;
            OnStart();
        }

        // Update is called once per frame
        void Update()
        {
            if (weapon!=null) weapon.wielder = this;
            OnUpdate();
        }
        public virtual void OnStart()
        {

        }
        public virtual void OnUpdate()
        {

        }
        public void Kill()
        {
            health = 0;
        }
        public virtual void Attack(int combo, float t)
        {
            if (weapon != null) weapon.Attack(combo, t);
        }
        public virtual void Damage(float amount)
        {
            health -= amount;
            //Debug.Log("Hit");
        }
        public virtual void OnHit(int side)
        {

        }
        public virtual float radius
        {
            get
            {
                return 0.2f;
            }
        }
        public virtual bool can_collide
        {
            get
            {
                return false;
            }
        }
        public virtual void Collide()
        {
            foreach (Entity e in entities)
            {
                if (e.can_collide && e != this)
                {
                    Vector3 to_other = e.position - position;
                    float min_dist = e.radius + radius;
                    if (to_other.magnitude < min_dist)
                    {
                        Vector3 v = to_other.normalized * min_dist;
                        position = e.position - v;
                    }
                }
            }
        }
        public virtual int team
        {
            get
            {
                return -1; // unassigned
            }
        }
        public virtual bool can_be_hit
        {
            get
            {
                return false;
            }
        }

    }
}