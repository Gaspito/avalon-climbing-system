using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic
{
    /// <summary>
    /// A base class that can make actors perform different actions, such as animations, audio cues, cutscenes, etc.
    /// </summary>
    public class ActorCommand
    {
        /// <summary>
        /// A flag telling the execution that this command is still running.
        /// Once it is set to false, then the next command can start.
        /// </summary>
        public bool _active = false;

        /// <summary>
        /// Overriden by the subtype of command. The default execution just runs the next action in the list.
        /// </summary>
        /// <param name="actor">The actor to control with this command.</param>
        /// <param name="list">The list of all commands.</param>
        /// <param name="id">The index of this current action in the list.</param>
        public virtual void Run(Actor actor, ActorCommandList list, int id)
        {
            _active = false;
            list.Next(actor, list, id);
        }
    }
}