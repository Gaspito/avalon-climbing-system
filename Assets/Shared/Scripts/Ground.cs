using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.PhysicsEngine
{
    public class Ground : MonoBehaviour
    {
        Collider _collider;

        void Start()
        {
            _collider = GetComponent<Collider>();
        }
    }
}