using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Logic;

namespace Game.Logic.Commands.Objectives
{
    public class AddMainToken : ActorCommand
    {
        Transform transform;
        string label;

        public AddMainToken(Transform transform, string label)
        {
            this.transform = transform;
            this.label = label;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            HUD.MainObjectiveMapToken.AddToken(label, transform);
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class RemoveMainToken : ActorCommand
    {
        bool all;
        string label;

        public RemoveMainToken()
        {
            this.all = true;
        }

        public RemoveMainToken(string label)
        {
            this.label = label;
            this.all = false;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            //Debug.Log(HUD.MainObjectiveMapToken.list.Count.ToString());
            if (all) HUD.MainObjectiveMapToken.RemoveAll();
            else HUD.MainObjectiveMapToken.RemoveToken(label);
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class RemoveMain : ActorCommand
    {

        public RemoveMain()
        {

        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            HUD.MainObjective.Remove();
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class SetMain : ActorCommand
    {
        string _description;

        public SetMain(string description)
        {
            _description = description;
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            HUD.MainObjective.Set(_description);
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class ShowMain : ActorCommand
    {

        public ShowMain()
        {
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            HUD.MainObjective.Show();
            _active = false;
            list.Next(actor, list, id);
        }
    }

    public class HideMain : ActorCommand
    {

        public HideMain()
        {
        }

        public override void Run(Actor actor, ActorCommandList list, int id)
        {
            HUD.MainObjective.Hide();
            _active = false;
            list.Next(actor, list, id);
        }
    }
}