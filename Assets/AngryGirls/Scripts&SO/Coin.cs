using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Angry_Girls
{
    public class Coin : MonoBehaviour, IPickable
    {
        [SerializeField] private int _coinValue = 1;

        [SerializeField] private Vector3 _rotationVector3 = new Vector3(0, 180, 90);
        [SerializeField] private float _duration = 1f;

        private Tween _tween;

        public void OnPickUp()
        {
            GameLoader.Instance.gameLogic_UIManager.UpdateScore(_coinValue);
        }

        private void OnTriggerEnter(Collider other)
        {
            OnPickUp();

            _tween.Kill();
            Destroy(gameObject);
        }


        void Start()
        {
            _tween = transform.DORotate(_rotationVector3, _duration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
            //transform.DORotate(new Vector3(360, 0, 0), 0.3f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
        }
    }
}