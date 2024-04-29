using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Commands.Movements
{
    public class MoveTo : ActorCommand
    {
        Transform _transform;
        Vector3 _target;
        bool _facing;
        float _speed;

        bool _wait;
        bool _triggered_next;

        public MoveTo(Transform transform, Vector3 target, bool facing, float speed, bool wait)
        {
            _transform = transform;
            _target = target;
            _facing = facing;
            _speed = speed;
            _wait = wait;
            _triggered_next = false;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            float dist = Vector3.Distance(_transform.position, _target);
            if (!_wait && !_triggered_next)
            {
                list.Next(actor, list, id);
                _triggered_next = true;
            }
            if (dist > _speed * Time.deltaTime * 2)
            {
                Vector3 to_target = _target - _transform.position;
                to_target = to_target.normalized * _speed * Time.deltaTime;
                _transform.position += to_target;
            } else
            {
                _transform.position = _target;
                _active = false;
                _triggered_next = false;
                if (_wait) list.Next(actor, list, id);
            }
        }
    }

    public class SetParent : ActorCommand
    {

        Transform _parent;
        Transform _target;
        bool _reset;

        public SetParent(Transform target, Transform parent, bool reset)
        {
            _target = target;
            _parent = parent;
            _reset = reset;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            if (_reset)
            {
                _target.position = _parent.position;
                _target.rotation = _parent.rotation;
            }
            _target.SetParent(_parent, true);
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class LockPlayer : ActorCommand
    {
        bool _lock_player;
        bool _lock_cam;

        public LockPlayer(bool lock_player)
        {
            _lock_player = lock_player;
            _lock_cam = lock_player;
        }
        public LockPlayer(bool lock_player, bool lock_cam)
        {
            _lock_player = lock_player;
            _lock_cam = lock_cam;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            if (_lock_player)
            {
                Player.main._can_move = false;
            }
            else
            {
                Player.main._can_move = true;
            }
            CameraAI.main._locked = _lock_cam;
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class Teleport : ActorCommand
    {
        Transform _target;
        Vector3 _position;
        Vector3 _look_dir;

        public Teleport(Transform target, Vector3 position, Vector3 look_dir)
        {
            _target = target;
            _position = position;
            _look_dir = look_dir;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            _target.position = _position;
            _target.rotation = Quaternion.LookRotation(_look_dir.normalized, Vector3.up);
            _active = false;
            list.Next(actor, list, id);
        }
    }
}