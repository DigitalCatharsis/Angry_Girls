using DG.Tweening;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Continuously rotates an object in local space
    /// </summary>
    public class LocalRotation : MonoBehaviour
    {
        [SerializeField] private Vector3 _rotationVector3;
        [SerializeField] private float _duration;

        private void Start()
        {

            transform.DORotate(_rotationVector3, _duration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
            //transform.DORotate(new Vector3(360, 0, 0), 0.3f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
        }
    }
}