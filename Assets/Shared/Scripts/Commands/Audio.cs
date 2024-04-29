using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Commands.Audio
{
    public class PlaySound : ActorCommand
    {
        AudioSource _source;
        AudioClip _clip;
        float _volume;
        bool _loop;
        bool _space;

        public PlaySound(AudioSource source, AudioClip clip, float volume, bool loop, bool space)
        {
            _source = source;
            _clip = clip;
            _volume = volume;
            _loop = loop;
            _space = space;

            _clip.LoadAudioData();
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            _source.clip = _clip;
            _source.volume = _volume;
            _source.loop = _loop;
            _source.spatialize = _space;
            _source.Play();
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class ChangeVolume : ActorCommand
    {
        AudioSource _source;
        float _target_volume;
        float _time;

        float _previous_volume;
        float _timer;

        public ChangeVolume(AudioSource source, float target_volume, float time)
        {
            _source = source;
            _target_volume = target_volume;
            _time = time;

            _timer = 0;
            _previous_volume = 0;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            if (_timer == 0)
            {
                _previous_volume = _source.volume;
                list.Next(actor, list, id);
            }
            if (_timer < _time)
            {
                _timer += Time.deltaTime;
                _source.volume = Mathf.Lerp(_previous_volume, _target_volume, _timer / _time);
            }
            else
            {
                _timer = 0;
                _active = false;
            }
        }
    }
}