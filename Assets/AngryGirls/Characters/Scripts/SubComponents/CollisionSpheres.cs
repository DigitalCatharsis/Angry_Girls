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

        [Header("Spheres")]
        public GameObject[] bottomSpheres = new GameObject[5];
        public GameObject[] frontSpheres = new GameObject[10];
        public GameObject[] backSpheres = new GameObject[10];
        public GameObject[] upSpheres = new GameObject[5];

        public override void OnAwake()
        {
        }
        public override void OnComponentEnable()
        {
            control.subComponentProcessor.collisionSpheres = this;
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
            //bottom
            for (int i = 0; i < 5; i++)
            {
                var sphere = LoadCollisionSpheres();
                sphere.transform.parent = _bottom;
                bottomSpheres[i] = sphere;
            }

            Reposition_BottomSpheres();

            //top
            for (int i = 0; i < 5; i++)
            {
                var sphere = LoadCollisionSpheres();
                sphere.transform.parent = _up;
                upSpheres[i] = sphere;
            }

            Reposition_UpSpheres();

            //front
            for (int i = 0; i < 10; i++)
            {
                var sphere = LoadCollisionSpheres();
                frontSpheres[i] = sphere;
                sphere.transform.parent = _front;
                //frontOverlapCheckers[i] = sphere.GetComponent<OverlapChecker>();
            }

            Reposition_FrontSpheres();

            //back
            for (int i = 0; i < 10; i++)
            {
                var sphere = LoadCollisionSpheres();
                sphere.transform.parent = _back;
                backSpheres[i] = sphere;
            }

            Reposition_BackSpheres();

            ////add every overlapChecker
            //var overlapCheckers_Array = this.gameObject.GetComponentsInChildren<OverlapChecker>();
            //collisionSpheres_Data.allOverlapCheckers = overlapCheckers_Array;
        }

        public void Reposition_FrontSpheres()
        {
            var bounds = new Bounds(control.boxCollider.center, control.boxCollider.size);

            var upPosition = new Vector3(0, bounds.max.y, bounds.max.z);
            var bottomPosition = new Vector3(0, bounds.min.y, bounds.max.z);

            frontSpheres[0].transform.localPosition = upPosition;
            frontSpheres[1].transform.localPosition = bottomPosition;

            float interval = (upPosition.y - bottomPosition.y + 0.05f) / 9;

            for (int i = 2; i < frontSpheres.Length; i++)
            {
                frontSpheres[i].transform.localPosition =
                    new Vector3(0f, upPosition.y - (interval * (i - 1)), upPosition.z);
            }
        }

        public void Reposition_BackSpheres()
        {
            var bounds = new Bounds(control.boxCollider.center, control.boxCollider.size);

            var upPosition = new Vector3(0, bounds.max.y, bounds.min.z);
            var bottomPosition = new Vector3(0, bounds.min.y, bounds.min.z);

            backSpheres[0].transform.localPosition = upPosition;
            backSpheres[1].transform.localPosition = bottomPosition;

            float interval = (upPosition.y - bottomPosition.y + 0.05f) / 9;

            for (int i = 2; i < backSpheres.Length; i++)
            {
                backSpheres[i].transform.localPosition =
                    new Vector3(0f, upPosition.y - (interval * (i - 1)), upPosition.z);
            }
        }

        public void Reposition_BottomSpheres()
        {
            {
                var bounds = new Bounds(control.boxCollider.center, control.boxCollider.size);

                var frontPosition = new Vector3(0, bounds.min.y, bounds.max.z);
                var backPosition = new Vector3(0, bounds.min.y, bounds.min.z);

                bottomSpheres[0].transform.localPosition = frontPosition;
                bottomSpheres[1].transform.localPosition = backPosition;

                float interval = (frontPosition.z - backPosition.z) / 4;

                for (int i = 2; i < bottomSpheres.Length; i++)
                {
                    bottomSpheres[i].transform.localPosition =
                        new Vector3(0f, frontPosition.y, frontPosition.z - (interval * (i - 1)));
                }
            }
        }

        public void Reposition_UpSpheres()
        {
            //var bounds = control.boxCollider.bounds;
            var bounds = new Bounds(control.boxCollider.center, control.boxCollider.size);

            var frontPosition = new Vector3(0, bounds.max.y, bounds.max.z);
            var backPosition = new Vector3(0, bounds.max.y, bounds.min.z);

            upSpheres[0].transform.localPosition = frontPosition;
            upSpheres[1].transform.localPosition = backPosition;

            float interval = (frontPosition.z - backPosition.z + 0.05f) / 4;

            for (int i = 2; i < upSpheres.Length; i++)
            {
                upSpheres[i].transform.localPosition =
                    new Vector3(0f, frontPosition.y, frontPosition.z - (interval * (i - 1)));
            }
        }

        public override void OnUpdate()
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



        //OverlapChecker[] frontOverlapCheckers = new OverlapChecker[10];
        //public override void OnFixedUpdate()
        //{
        //    for (int i = 0; i < collisionSpheres_Data.allOverlapCheckers.Length; i++)
        //    {
        //        collisionSpheres_Data.allOverlapCheckers[i].UpdateChecker();
        //    }
        //}


    }
}