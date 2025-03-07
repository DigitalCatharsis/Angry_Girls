using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Angry_Girls
{
    public class CollisionSpheres : SubComponent
    {
        [Header("Setup")]
        [SerializeField] private Transform _up;
        [SerializeField] private Transform _bottom;
        [SerializeField] private Transform _front;
        [SerializeField] private Transform _back;

        private Bounds _bounds;

        public override void OnComponentEnable()
        {
            _bounds = new Bounds(control.boxCollider.center, control.boxCollider.size);
            SetColliderSpheres();
        }

        private GameObject LoadCollisionSpheres()
        {
            return Instantiate(Resources.Load("CollisionSphere", typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
        }

        //private void OnDrawGizmos()
        //{
        //    var boxCollider = gameObject.transform.root.GetComponent<BoxCollider>();

        //    var localBounds = new Bounds(boxCollider.center, boxCollider.size);

        //    const float radius = 0.1f;

        //    Gizmos.color = Color.cyan;

        //    Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.min.y, localBounds.min.z)), radius);
        //    Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.min.y, localBounds.max.z)), radius);
        //    Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.max.z)), radius);
        //    Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.min.z)), radius);

        //    Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.min.z)), radius);
        //    Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.max.z)), radius);
        //    Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.max.y, localBounds.max.z)), radius);
        //    Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.max.y, localBounds.min.z)), radius);
        //}

        private void SetColliderSpheres()
        {
            InitializeSpheres(control.collisionSpheresData.bottomSpheres, _bottom, 5, Reposition_BottomSpheres);
            InitializeSpheres(control.collisionSpheresData.upSpheres, _up, 5, Reposition_UpSpheres);
            InitializeSpheres(control.collisionSpheresData.frontSpheres, _front, 10, Reposition_FrontSpheres);
            InitializeSpheres(control.collisionSpheresData.backSpheres, _back, 10, Reposition_BackSpheres);
        }

        private void InitializeSpheres(GameObject[] spheres, Transform parent, int count, System.Action repositionMethod)
        {
            for (int i = 0; i < count; i++)
            {
                var sphere = LoadCollisionSpheres();
                sphere.transform.parent = parent;
                spheres[i] = sphere;
            }
            repositionMethod();
        }

        public void RepositionAllSpheres()
        {
            Reposition_FrontSpheres();
            Reposition_BottomSpheres();
            Reposition_BackSpheres();
            Reposition_UpSpheres();
        }
        public void Reposition_FrontSpheres()
        {
            var upPosition = new Vector3(0, _bounds.max.y, _bounds.max.z);
            var bottomPosition = new Vector3(0, _bounds.min.y, _bounds.max.z);

            control.collisionSpheresData.frontSpheres[0].transform.localPosition = upPosition;
            control.collisionSpheresData.frontSpheres[1].transform.localPosition = bottomPosition;

            float interval = (upPosition.y - bottomPosition.y + 0.05f) / 9;

            for (int i = 2; i < control.collisionSpheresData.frontSpheres.Length; i++)
            {
                control.collisionSpheresData.frontSpheres[i].transform.localPosition =
                    new Vector3(0f, upPosition.y - (interval * (i - 1)), upPosition.z);
            }
        }

        public void Reposition_BackSpheres()
        {
            var upPosition = new Vector3(0, _bounds.max.y, _bounds.min.z);
            var bottomPosition = new Vector3(0, _bounds.min.y, _bounds.min.z);

            control.collisionSpheresData.backSpheres[0].transform.localPosition = upPosition;
            control.collisionSpheresData.backSpheres[1].transform.localPosition = bottomPosition;

            float interval = (upPosition.y - bottomPosition.y + 0.05f) / 9;

            for (int i = 2; i < control.collisionSpheresData.backSpheres.Length; i++)
            {
                control.collisionSpheresData.backSpheres[i].transform.localPosition =
                    new Vector3(0f, upPosition.y - (interval * (i - 1)), upPosition.z);
            }
        }

        public void Reposition_BottomSpheres()
        {
            var frontPosition = new Vector3(0, _bounds.min.y, _bounds.max.z);
            var backPosition = new Vector3(0, _bounds.min.y, _bounds.min.z);

            control.collisionSpheresData.bottomSpheres[0].transform.localPosition = frontPosition;
            control.collisionSpheresData.bottomSpheres[1].transform.localPosition = backPosition;

            float interval = (frontPosition.z - backPosition.z) / 4;

            for (int i = 2; i < control.collisionSpheresData.bottomSpheres.Length; i++)
            {
                control.collisionSpheresData.bottomSpheres[i].transform.localPosition =
                    new Vector3(0f, frontPosition.y, frontPosition.z - (interval * (i - 1)));
            }
        }

        public void Reposition_UpSpheres()
        {
            var frontPosition = new Vector3(0, _bounds.max.y, _bounds.max.z);
            var backPosition = new Vector3(0, _bounds.max.y, _bounds.min.z);

            control.collisionSpheresData.upSpheres[0].transform.localPosition = frontPosition;
            control.collisionSpheresData.upSpheres[1].transform.localPosition = backPosition;

            float interval = (frontPosition.z - backPosition.z + 0.05f) / 4;

            for (int i = 2; i < control.collisionSpheresData.upSpheres.Length; i++)
            {
                control.collisionSpheresData.upSpheres[i].transform.localPosition =
                    new Vector3(0f, frontPosition.y, frontPosition.z - (interval * (i - 1)));
            }
        }

        public override void OnUpdate()
        {
        }

        public override void OnAwake()
        {
        }

        public override void OnFixedUpdate()
        {
        }

        public override void OnStart()
        {
        }

        public override void OnLateUpdate()
        {
        }
    }

    [Serializable]
    public class CollisionSpheresData
    {
        public GameObject[] bottomSpheres = new GameObject[5];
        public GameObject[] frontSpheres = new GameObject[10];
        public GameObject[] backSpheres = new GameObject[10];
        public GameObject[] upSpheres = new GameObject[5];
    }
}