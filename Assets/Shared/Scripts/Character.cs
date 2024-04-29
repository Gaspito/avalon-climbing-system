using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Characters
{
    [System.Serializable]
    public class CharacterPose
    {
        public string _name = "new";
        public AnimationClip _clip;
        public float _crossfade = 0.1f;
    }

    [System.Serializable]
    public class CharacterSound
    {
        public string name = "new";
        public AudioClip clip;
        public float volume = 1;
        public CharacterSound(string n, AudioClip c, float v)
        {
            name = n;
            clip = c;
            volume = v;
        }
    }

    public class Character : MonoBehaviour
    {
        public Animation _animator;
        public List<CharacterPose> _pose_list = new List<CharacterPose>();
        public List<AudioSource> _sources;

        // Use this for initialization
        void Start()
        {
            foreach (CharacterPose p in _pose_list)
            {
                _animator.AddClip(p._clip, p._name);
            }
            if (_sources == null) _sources = new List<AudioSource>(){ };
        }

        public void UpdatePoses()
        {
            foreach (CharacterPose p in _pose_list)
            {
                if (_animator.GetClip(p._name) != null)
                {
                    _animator.RemoveClip(p._name);
                }
                _animator.AddClip(p._clip, p._name);
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateSounds();
        }

        void UpdateSounds()
        {
            for (int i = 0; i < _sources.Count; i++)
            {
                if (!_sources[i].isPlaying)
                {
                    AudioSource.Destroy(_sources[i]);
                    _sources.RemoveAt(i);
                    break;
                }
            }
        }

        public CharacterPose GetPose(string label)
        {
            foreach (CharacterPose p in _pose_list)
            {
                if (p._name == label) return p;
            }
            return null;
        }

        public void Pose(string label)
        {
            CharacterPose p = GetPose(label);
            if (p == null) return;
            _animator.CrossFade(p._name, p._crossfade);
        }

        public List<CharacterSound> _sounds = new List<CharacterSound>();

        public CharacterSound GetSound(string label)
        {
            foreach (CharacterSound s in _sounds)
            {
                if (s.name == label) return s;
            }
            return null;
        }

        public void PlaySound(string label)
        {
            CharacterSound s = GetSound(label);
            if (s != null)
            {
                AudioSource a = gameObject.AddComponent<AudioSource>();
                a.clip = s.clip;
                a.volume = s.volume;
                a.spatialize = true;
                a.spatialBlend = 1;
                a.Play();
                _sources.Add(a);
            }
        }
    }
}