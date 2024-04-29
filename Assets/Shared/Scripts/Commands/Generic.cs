using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Commands.Generic
{
    public class Wait : ActorCommand
    {
        float _time;

        float _timer;
        
        public Wait(float time)
        {
            _time = time;
            _timer = 0;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            if (_timer < _time - Time.deltaTime)
            {
                _timer += Time.deltaTime;
            } else
            {
                _active = false;
                _timer = 0;
                list.Next(actor, list, id);
            }
        }
    }


}