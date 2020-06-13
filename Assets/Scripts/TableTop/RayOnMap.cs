
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TableTop
{
    public class RayOnMap : MonoBehaviour
    {
        public delegate void OnMeshHit(int i);
        public static event OnMeshHit MeshHit;

        [SerializeField]
        public bool staticData = false;

        [SerializeField]
        public MeshFilter Mesh;

 
        [SerializeField]
        private Collider _mapCollider;
        private Collider MapCollider
        {
            get { return _mapCollider; }
            set { _mapCollider = value; }
        }

        private RaycastHit hit;

        private Ray ray;


        public Nullable<Vector3> MouseRay()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (_mapCollider.Raycast(ray, out hit, 200f))  return hit.point;

            return null;
        }

        public Nullable<Vector3> HeadRay()
        {
            if (inputsManager.Instance.LocalHead == null) return null;

            return RayCollision(inputsManager.Instance.LocalHead);

        }

        public Nullable<Vector3> RemoteHeadRay()
        {
            if (inputsManager.Instance.RemoteHead == null) return null;

            return RayCollision(inputsManager.Instance.RemoteHead);

        }

        public Nullable<Vector3> ControllerRay()
        {
            if (inputsManager.Instance.Controller == null) return null;

            return RayCollision(inputsManager.Instance.Controller);

        }

        public Nullable<Vector3> RemoteControllerRay()
        {
            if (inputsManager.Instance.RemoteController == null) return null;

            return RayCollision(inputsManager.Instance.RemoteController);

        }

        private Nullable<Vector3> RayCollision(Transform t) {

            Ray ray = new Ray(t.position, t.forward);

            if (_mapCollider.Raycast(ray, out hit, 200f))  return hit.point;

            return null;

        }


    }
}
