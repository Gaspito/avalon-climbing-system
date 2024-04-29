using Game.Logic.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Weapons
{
    public class Excalibur : Weapon
    {
        public override float damage
        {
            get
            {
                return 35;
            }
        }
        public override void Attack(int combo, float t)
        {
            if (combo == 0) Combo_0(t);
        }

        bool IsEntityInHitZone(Entity e, float radius, float angle)
        {
            Vector3 to_e = e.position - wielder.position;
            if (to_e.magnitude <= radius && Vector3.Angle(to_e, wielder.direction) <= angle)
                return true;
            else
                return false;
        }

        public TrailRenderer trail;
        public bool attacking = false;

        public override void OnUpdate()
        {
            if (!attacking)
            {
                if (trail != null)
                {
                    //trail.transform.SetParent(null);
                    //trail = null;
                }
            }
            attacking = false;
        }

        float[] combo_duration = new float[] { 0.4f};
        float[] hit_impact = new float[] { 0.2f };
        float[] hit_radiuses = new float[] { 2f };
        float[] hit_angles = new float[] { 45 };
        string[] combo_pose = new string[] { "combo_0"};

        void Combo_0(float t)
        {
            if (t-Time.deltaTime<= 0)
            {
                FindNearestFoe(wielder.position + wielder.direction.normalized * hit_radiuses[0] * 0.5f);
            }
            FaceFoe();

            wielder.Move(wielder.direction, 3f);

            //current_pose = combo_pose[0];
            if (t <= hit_impact[0] && t + Time.deltaTime >= hit_impact[0])
            {
                Hit(hit_radiuses[0], hit_angles[0]);
            }

            attacking = true;

        }

        Entity main_foe;

        void FindNearestFoe(Vector3 pos)
        {
            float d = -1;
            foreach (Entity e in Entity.entities)
            {
                if (e != wielder && e.team != wielder.team && e.can_be_hit && !e.dead)
                {
                    Vector3 to_foe = e.position - pos;
                    to_foe.y = 0;
                    if (to_foe.magnitude < d || d < 0)
                    {
                        main_foe = e;
                        d = to_foe.magnitude;
                    }
                }
            }
        }

        void FaceFoe()
        {
            if (main_foe != null && !main_foe.dead && main_foe.can_be_hit && main_foe.team!= wielder.team)
            {
                Vector3 to_foe = main_foe.position - wielder.position;
                to_foe.y = 0;
                wielder.direction = to_foe;
            }
        }

        void Hit(float radius, float angle)
        {
            foreach (Entity e in Entity.entities)
            {
                if (e != wielder && e.team != wielder.team && e.can_be_hit)
                {
                    if (IsEntityInHitZone(e, radius, angle))
                    {
                        e.Damage(damage);
                        e.OnHit(1);
                    }
                }
            }
        }

    }
}