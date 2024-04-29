using Game.Logic.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic.Weapons
{

    /// <summary>
    /// Base class for every weapon of the game.
    /// Each attack is done with a weapon object (the weapon can be the entity's hands).
    /// Weapons determine the damage dealt and callbacks during the attack.
    /// They can also have animations (and rigs) of their own if a character object is linked to it.
    /// </summary>
    public class Weapon : MonoBehaviour
    {
        /// <summary>
        /// The entity currently using the weapon.
        /// </summary>
        public Entity wielder;
        /// <summary>
        /// The rig and animations controller of this weapon, if any.
        /// </summary>
        public Character character;
        /// <summary>
        /// The current animation of the weapon, if any.
        /// </summary>
        public string current_pose = "idle";

        // Use this for initialization
        void Start()
        {
            OnStart();
        }

        // Update is called once per frame
        void Update()
        {
            OnUpdate();
            if (character!=null) character.Pose(current_pose);
        }

        public virtual void OnStart()
        {

        }
        public virtual void OnUpdate()
        {

        }
        /// <summary>
        /// Overriden to apply different base damage per weapon.
        /// </summary>
        public virtual float damage
        {
            get
            {
                return 1;
            }
        }
        /// <summary>
        /// Overriden to apply effects uppon attacking.
        /// </summary>
        /// <param name="combo">The index of the combo used.</param>
        /// <param name="t">The time of the attack.</param>
        public virtual void Attack(int combo, float t)
        {

        }
        

    }
}