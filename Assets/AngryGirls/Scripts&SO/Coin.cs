using DG.Tweening;
using UnityEngine;

namespace Angry_Girls
{
    public class Coin : MonoBehaviour, IPickable
    {
        [SerializeField] private int _coinValue = 1;
        [SerializeField] private Vector3 _rotationVector3 = new Vector3(0, 180, 90);
        [SerializeField] private float _duration = 1f;

        private Tween _tween;

        public int Value => _coinValue;

        private void Start()
        {
            _tween = transform.DORotate(_rotationVector3, _duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);

            // Регистрируем монету в InteractionManager
            GameLoader.Instance.interactionManager.Register(
                gameObject,
                new InteractionConfig
                {
                    type = InteractionMemberType.Pickable,
                });
        }

        private void Dispose()
        {
            _tween.Kill();
            GameLoader.Instance.interactionManager.Unregister(gameObject);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_tween != null && _tween.IsActive())
            {
                _tween.Kill();
            }
        }

        public void OnPickUp()
        {
            GameLoader.Instance.gameLogic_UIManager.UpdateScore(_coinValue);
            GameLoader.Instance.audioManager.PlayCustomSound(AudioSourceType.Coins, 1, false);
            GameLoader.Instance.VFXManager.ShowCoinsValue(transform.position, _coinValue);

            Dispose();
        }
    }
}