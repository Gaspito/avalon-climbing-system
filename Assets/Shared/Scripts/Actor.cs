using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic
{
    public class Actor : MonoBehaviour
    {
        static GUISkin _gui_skin;
        public static GUISkin GetSkin()
        {
            if (_gui_skin == null) _gui_skin = Resources.Load<GUISkin>("GUI/Actor");
            return _gui_skin;
        }

        public static List<Actor> ALL_ACTORS = new List<Actor>();

        public virtual string GetName()
        {
            return "New Actor";
        }
        public virtual float GetEditorDrawDistance()
        {
            return 10;
        }

        public List<ActorCommandList> _commands;

        public void AddCommandList(string label, ActorCommandList.OnCompletionDelegate on_completion, params ActorCommand[] p)
        {
            _commands.Add(new ActorCommandList(label, on_completion, p));
        }

        public ActorCommandList FindCommandList(string label)
        {
            foreach (ActorCommandList l in _commands)
            {
                if (l._label == label) return l;
            }
            return null;
        }

        public int _state=-1;

        // Use this for initialization
        void Start()
        {
            ALL_ACTORS.Add(this);
            _state = -1;
            _commands = new List<ActorCommandList>();
            //OnStart();
            OnAwake();
        }

        public virtual void OnAwake()
        {

        }

        void UpdateCommands()
        {
            foreach(ActorCommandList l in _commands)
            {
                l.Run(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_state < 0)
            {
                OnStart();
                _state = 0;
            }
            UpdateCommands();
            OnUpdate();
        }

        public virtual void OnStart()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public void DoNothing(Actor actor, ActorCommandList list)
        {

        }
        public void NextState(Actor actor, ActorCommandList list)
        {
            _state++;
        }

        public void Loop(Actor actor, ActorCommandList list)
        {
            list.Start();
        }

    }

    public class ActorCommandList
    {
        public string _label;

        public List<ActorCommand> _commands;

        public delegate void OnCompletionDelegate(Actor actor, ActorCommandList list);
        public event OnCompletionDelegate OnCompletion;

        public ActorCommandList(string label, OnCompletionDelegate on_completion, params ActorCommand[] p)
        {
            _label = label;
            OnCompletion = on_completion;
            _commands = new List<ActorCommand>();
            foreach(ActorCommand c in p)
            {
                _commands.Add(c);
            }
        }

        public bool Active
        {
            get
            {
                foreach (ActorCommand c in _commands)
                {
                    if (c._active) return true;
                }
                return false;
            }
        }

        public void Run(Actor actor)
        {
            for (int i = 0; i < _commands.Count;i++)
            {
                ActorCommand command = _commands[i];
                if (command._active) command.Run(actor, this, i);
            }
        }

        public void Start()
        {
            if (_commands.Count > 0) _commands[0]._active = true;
            else Debug.LogError("Command List is empty.");
        }

        public void Stop()
        {
            foreach (ActorCommand c in _commands)
            {
                c._active = false;
            }
        }

        public void End(Actor actor, ActorCommandList list)
        {
            if (OnCompletion != null) OnCompletion(actor, list);
        }

        public void Next(Actor actor, ActorCommandList list, int id)
        {
            if (id + 1 < _commands.Count) _commands[id + 1]._active = true;
            else End(actor, list);
        }
    }
}