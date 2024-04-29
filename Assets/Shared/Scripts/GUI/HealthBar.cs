using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Logic;
using Game.Logic.Entities;

namespace Game.HUD
{
    public class HealthBar : MonoBehaviour
    {
        Entity_Player entity;
        public Image bar;

        // Use this for initialization
        void Start()
        {
            entity = Player.main.entity;
        }

        // Update is called once per frame
        void Update()
        {
            //entity.Damage(0.1f);
            bar.fillAmount = entity.health / entity.max_health;
        }
    }
}