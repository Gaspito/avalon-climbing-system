using Game.Logic.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Weapons
{
    public class ShadowClaws : Weapon
    {
        public override float damage
        {
            get
            {
                return 10;
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

        float[] hit_impact = new float[] { 0.7f };
        float[] hit_radiuses = new float[] { 1.5f };
        float[] hit_angles = new float[] { 30 };
        string[] combo_pose = new string[] { "combo_0"};

        void Combo_0(float t)
        {
            //current_pose = combo_pose[0];
            if (t <= hit_impact[0] && t + Time.deltaTime >= hit_impact[0])
            {
                Hit(hit_radiuses[0], hit_angles[0]);
            }
            
        }

        void Hit(float radius, float angle)
        {
            foreach (Entity e in Entity.entities)
            {
                if (e != wielder&&e.team!=wielder.team)
                {
                    if (IsEntityInHitZone(e, radius, angle))
                    {
                        e.Damage(damage);
                        e.OnHit(2);
                    }
                }
            }
        }

    }
}