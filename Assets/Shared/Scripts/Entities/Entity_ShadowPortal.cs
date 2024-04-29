using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Entities
{
    public class Entity_ShadowPortal : Entity
    {
        public override bool can_collide
        {
            get
            {
                return false;
            }
        }
        public override int team
        {
            get
            {
                return 1;
            }
        }
        public override bool can_be_hit
        {
            get
            {
                return false;
            }
        }
        public override void Damage(float amount)
        {
            
        }
        float death_time = 5;
        float death_particles_time = 2;
        bool death_particles_spawned = false;
        float death_timer = 0;
        public ParticleSystem particles1;
        public ParticleSystem particles2;
        public override void OnUpdate()
        {
            if (dead)
            {
                death_timer += Time.deltaTime;
                if (death_timer >= death_time)
                {
                    entities.Remove(this);
                    Destroy(gameObject);
                } else if (death_timer >= death_particles_time && !death_particles_spawned)
                {
                    particles1.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    particles2.Emit(25);
                }
            }
            base.OnUpdate();
        }
    }
}