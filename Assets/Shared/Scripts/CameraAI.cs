using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Logic
{
    public class CameraAI : MonoBehaviour
    {
        public static CameraAI main;

        public bool _locked = false;
        public float _x_angle = 0;
        public float _y_angle = 0;
        public float _dist_to_target = 5;
        public float _max_y_angle = 60;
        public float _sensibility = 0.5f;

        public Transform _target;
        public RenderTexture _CustomShadowsTexture;

        private void Awake()
        {
            main = this;
        }
        // Use this for initialization
        void Start()
        {
            InitCustomShadows();
        }

        Camera _cam;
        Camera _CustomShadowsCam;

        void InitCustomShadows()
        {
            _cam = GetComponent<Camera>();
            float ratio = 16.0f / 9.0f;
            int size = 1024;
            int w = Mathf.RoundToInt( size * ratio);
            _CustomShadowsTexture = new RenderTexture(w, size, 16, RenderTextureFormat.Depth);

            _CustomShadowsTexture.Create();

            GameObject obj = new GameObject("CustomShadowsRenderer");
            _CustomShadowsCam = obj.AddComponent<Camera>();
            _CustomShadowsCam.clearFlags = CameraClearFlags.SolidColor;
            _CustomShadowsCam.backgroundColor = Color.black;
            _CustomShadowsCam.targetTexture = _CustomShadowsTexture;
            _CustomShadowsCam.cullingMask = LayerMask.GetMask("CustomShadows");
            _CustomShadowsCam.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            LockCursor();
            UpdateMovements();
            UpdateCollisions();
            UpdateView();
        }

        private void OnPreRender()
        {
            UpdateCustomShadows();

        }

        private void OnPostRender()
        {
            Shader.SetGlobalFloat("_CustomShadowsStrength", 0);

        }

        void UpdateCustomShadows()
        {
            _CustomShadowsCam.transform.position = transform.position;
            _CustomShadowsCam.transform.rotation = transform.rotation;
            _CustomShadowsCam.fieldOfView = _cam.fieldOfView;
            _CustomShadowsCam.farClipPlane = _cam.farClipPlane;
            _CustomShadowsCam.nearClipPlane = _cam.nearClipPlane;
            _CustomShadowsCam.Render();
            Shader.SetGlobalTexture("_CustomShadowsTex", _CustomShadowsTexture);
            Shader.SetGlobalFloat("_CustomShadowsStrength", 1);
        }

        void LockCursor()
        {
            if (!_locked) Cursor.lockState = CursorLockMode.Locked;
            else Cursor.lockState = CursorLockMode.None;
        }

        void UpdateMovements()
        {
            if (_locked) return;
            if (_target == null) return;
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            _x_angle -= x * _sensibility;
            _y_angle += y * _sensibility;

            if (_y_angle < _max_y_angle) _y_angle = _max_y_angle;
            if (_y_angle > 180-_max_y_angle) _y_angle = 180- _max_y_angle;

            Vector2 x_plane = new Vector2(Mathf.Cos(Mathf.Deg2Rad * _x_angle), Mathf.Sin(Mathf.Deg2Rad * _x_angle));
            Vector2 y_plane = new Vector2(Mathf.Sin(Mathf.Deg2Rad * _y_angle), Mathf.Cos(Mathf.Deg2Rad * _y_angle));
            Vector3 pos = new Vector3(x_plane.x, 0, x_plane.y);
            pos = pos.normalized * y_plane.x + Vector3.up * y_plane.y;
            transform.position = _target.position + pos.normalized * _dist_to_target;
        }

        void UpdateView()
        {
            if (_locked || _target == null) return;
            Vector3 to_target = _target.position - transform.position;
            to_target.Normalize();
            Vector3 r = transform.right;
            Vector3 up = Vector3.Cross(to_target, r.normalized);
            Quaternion view = Quaternion.LookRotation(to_target);
            transform.rotation = view;
        }

        void UpdateCollisions()
        {
            if (_locked || _target == null) return;
            Vector3 to_target = transform.position - _target.position;
            Ray r = new Ray(_target.position, to_target);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, _dist_to_target))
            {
                transform.position = hit.point - to_target.normalized * 0.3f;
                //Debug.Log("hit");
            }
        }
    }
}