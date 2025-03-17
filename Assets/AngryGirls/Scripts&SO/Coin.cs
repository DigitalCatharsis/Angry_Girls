using DG.Tweening;
using TMPro;
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
            GameLoader.Instance.audioManager.PlayCustomSound(AudioSourceType.Coins, 1, false);
            ShowCoinsValue();
        }


        public void ShowCoinsValue()
        {
            var previewVfx = GameLoader.Instance.VFXManager.SpawnVFX_AtPosition(VFX_Type.VFX_CoinValue, transform.position, Quaternion.identity).GetComponent<VFX>();
            previewVfx.InitAndRunVFX_ByCustom(
                timeToLive: 1,
                isTimeToLiveIsNormilizedTime: false,
                destroyOnCollision: false,
                destroyOnCharacterCollision: false,
                damage: 0,
                knockbackValue: 0,
                enableCollider: false,
                enableTrigger: false);

            previewVfx.GetComponentInChildren<TextMeshPro>().text = _coinValue.ToString();
            previewVfx.transform.DOMove(new Vector3(previewVfx.transform.position.x, previewVfx.transform.position.y + 1, previewVfx.transform.position.z), 1);
        }

        void Start()
        {
            _tween = transform.DORotate(_rotationVector3, _duration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
        }
    }
}