using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.HUD
{
    public class MainObjective : MonoBehaviour
    {
        public static MainObjective main;
        public Image panel;
        public Image token;
        public Text text;
        public bool show = false;
        public string description;

        // Use this for initialization
        void Start()
        {
            main = this;
        }

        // Update is called once per frame
        void Update()
        {
            panel.enabled = show;
            token.enabled = show;
            text.enabled = show;
            text.text = description;
        }
        public static void Set(string s)
        {
            main.description = s;
            Show();
        }
        public static void Show()
        {
            main.show = true;
        }
        public static void Hide()
        {
            main.show = false;
        }
        public static void Remove()
        {
            main.description = "";
            Hide();
        }
    }
}