using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Logic;

namespace Game.HUD
{
    public class MainObjectiveMapToken : MonoBehaviour
    {
        public static List<MainObjectiveMapToken> list = new List<MainObjectiveMapToken>();

        static string rsrc = "Prefabs/MainObjectiveToken";

        public static void AddToken(string label, Transform target)
        {
            MainObjectiveMapToken prefab = Resources.Load<MainObjectiveMapToken>(rsrc);
            MainObjectiveMapToken obj = Instantiate<GameObject>(prefab.gameObject, HUD.main.screen.transform, false).GetComponent<MainObjectiveMapToken>();
            obj.label = label;
            obj._objective = target;
            list.Add(obj);
        }

        public static void RemoveToken(string label)
        {
            foreach (MainObjectiveMapToken t in list)
            {
                if (t.name == label)
                {
                    t.Remove();
                    list.Remove(t);
                    break;
                }
            }
        }

        public static void RemoveAll()
        {
            foreach (MainObjectiveMapToken t in list)
            {
                t.Remove();
            }
            list.Clear();
        }

        public string label;
        public Image _token;
        public Transform _objective;
        RectTransform rect;

        // Use this for initialization
        void Start()
        {
            rect = GetComponent<RectTransform>();
        }

        public void Remove()
        {
            Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            Camera cam = Camera.main;
            if (_objective != null && MainObjective.main.show)
            {
                _token.enabled = true;
                Vector3 token_screenpos = cam.WorldToScreenPoint(_objective.position);
                Vector3 cam2token = _objective.position - cam.transform.position;
                float a = Vector3.Angle(cam2token, cam.transform.forward);
                if (a > 90)
                {
                    Vector3 screen_center = new Vector3(cam.pixelWidth, cam.pixelHeight, 1) * 0.5f;
                    Vector3 center2pos = token_screenpos - screen_center;
                    token_screenpos = screen_center + center2pos.normalized * (cam.pixelWidth + cam.pixelHeight);
                }
                token_screenpos.x = Mathf.Clamp(token_screenpos.x, rect.rect.width * 0.5f, cam.pixelWidth - rect.rect.width * 0.5f);
                token_screenpos.y = Mathf.Clamp(token_screenpos.y, rect.rect.height * 0.5f, cam.pixelHeight - rect.rect.height * 0.5f);
                transform.position = new Vector3(token_screenpos.x, token_screenpos.y, 1);
            }
            else
            {
                _token.enabled = false;
            }
        }
    }
}