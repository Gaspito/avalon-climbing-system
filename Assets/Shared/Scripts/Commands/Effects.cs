using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Logic;

namespace Game.Logic.Commands.Effects
{
    public class EmitParticles : ActorCommand
    {
        ParticleSystem _system;
        int _amount;

        public EmitParticles(ParticleSystem system, int amount)
        {
            _system = system;
            _amount = amount;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            _system.Emit(_amount);
            _active = false;
            list.Next(actor, list, id);
        }
    }
}