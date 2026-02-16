using DG.Tweening;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Represents a collectible coin in the game world
    /// </summary>
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
            GameplayCoreManager.Instance.OnInitialized += RegisterInInteractionManager;
        }

        private void RegisterInInteractionManager()
        {
            GameplayCoreManager.Instance.InteractionManager.Register(
                gameObject,
                new InteractionConfig
                {
                    type = InteractionMemberType.Pickable,
                });
        }

        private void Dispose()
        {
            _tween.Kill();
            GameplayCoreManager.Instance.InteractionManager.Unregister(gameObject);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_tween != null && _tween.IsActive())
            {
                GameplayCoreManager.Instance.OnInitialized -= RegisterInInteractionManager;
                _tween.Kill();
            }
        }

        /// <summary>
        /// Called when the coin is picked up by a playerz
        /// </summary>
        public void OnPickUp()
        {
            CoreManager.Instance.AudioManager.PlayCustomSound(AudioSourceType.Coins, 1, false);
            CoreManager.Instance.VFXManager.ShowCoinsValue(transform.position, _coinValue);

            Dispose();
        }
    }
}