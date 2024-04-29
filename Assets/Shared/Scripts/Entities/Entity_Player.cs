using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Entities
{
    public class Entity_Player : Entity
    {
        public Player player;
        public override float max_health
        {
            get
            {
                return 100;
            }
        }
        public override Vector3 position
        {
            get
            {
                return player.position;
            }

            set
            {
                player.position = value;
            }
        }
        public override Vector3 direction
        {
            get
            {
                return player.direction;
            }

            set
            {
                player.direction = value;
            }
        }
        public override void Move(Vector3 direction, float speed)
        {
            player._motion += direction.normalized * speed;
        }
        public override void OnHit(int side)
        {
            player.OnHit(side);
            //base.OnHit(side);
        }
        public override void Damage(float amount)
        {
            if (!player._hurt) base.Damage(amount);
        }
        public override bool can_collide
        {
            get
            {
                return true;
            }
        }
        public override float radius
        {
            get
            {
                return player._radius;
            }
        }
        public override int team
        {
            get
            {
                return 0; //player's team
            }
        }
        public override bool can_be_hit
        {
            get
            {
                return true;
            }
        }
    }
}