using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.HUD
{
    public class HUD : MonoBehaviour
    {
        public static HUD main;
        public Canvas screen;

        private void Awake()
        {
            main = this;
        }

        // Use this for initialization
        void Start()
        {
            main = this;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}