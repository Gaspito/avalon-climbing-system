using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Logic;
using Game.Logic.Characters;

namespace Game.Logic.Commands.Animations
{
    public class Animate : ActorCommand
    {
        string _clip_name;
        AnimationClip _clip;
        Animation _animator;
        bool _loop;

        public Animate(Animation animator, AnimationClip clip, string clip_name, bool loop)
        {
            _clip_name = clip_name;
            _clip = clip;
            _animator = animator;
            _loop = loop;

            if (_animator.GetClip(_clip_name) == null)
            {
                _animator.AddClip(_clip, _clip_name);
            }
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            _animator.Play(_clip_name);
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class SetCharacterAnimation : ActorCommand
    {
        string _label;
        AnimationClip _clip;
        float _crossfade;
        Character _character;

        public SetCharacterAnimation(Character character, string label, AnimationClip clip, float crossfade)
        {
            _character = character;
            _label = label;
            _clip = clip;
            _crossfade = crossfade;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            CharacterPose pose = _character.GetPose(_label);
            if (pose != null)
            {
                pose._clip = _clip;
                pose._crossfade = _crossfade;
            }
            else
            {
                pose = new CharacterPose();
                pose._name = _label;
                pose._clip = _clip;
                pose._crossfade = _crossfade;
                _character._pose_list.Add(pose);
            }
            _character.UpdatePoses();
            _active = false;
            list.Next(actor, list, id);
        }
    }
}