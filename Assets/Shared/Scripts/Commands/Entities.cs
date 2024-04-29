using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.Logic.Entities;
using Game.Logic.Weapons;

namespace Game.Logic.Commands.Entities
{
    public class SpawnEntity : ActorCommand
    {
        Entity _entity;
        Vector3 _position;
        Vector3 _direction;
        
        public SpawnEntity(Entity entity, Vector3 position, Vector3 direction)
        {
            _entity = entity;
            _position = position;
            _direction = direction;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            Entity obj = GameObject.Instantiate(_entity,_position,Quaternion.LookRotation(_direction,Vector3.up));
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class SetWeapon : ActorCommand
    {
        public Entity _entity;
        public Weapon _weapon;

        public SetWeapon(Entity entity, Weapon weapon)
        {
            _entity = entity;
            _weapon = weapon;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            _entity.weapon = _weapon;
            _active = false;
            list.Next(actor, list, id);
            //base.Run(actor, list, id);
        }
    }

    public class KillTeam : ActorCommand
    {
        int _team = -1;

        public KillTeam(int team)
        {
            _team = team;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            foreach (Entity e in Entity.entities)
            {
                if (e.team == _team)
                {
                    e.Kill();
                }
            }
            _active = false;
            list.Next(actor, list, id);
        }
    }
}