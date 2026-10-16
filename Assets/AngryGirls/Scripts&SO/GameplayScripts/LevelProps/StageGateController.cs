using System.Collections;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Slides a level part (for example a portcullis or a keep door) when the configured stage starts.
    /// Place it on the object that should move; add a kinematic Rigidbody to it so attached colliders move correctly.
    /// </summary>
    [DisallowMultipleComponent]
    public class StageGateController : MonoBehaviour
    {
        [Tooltip("The gate opens as soon as the stage with this index (or any later one) is set.")]
        [SerializeField, Min(0)] private int _openOnStageIndex = 1;

        [Tooltip("World-space offset applied to the gate when it is open.")]
        [SerializeField] private Vector3 _openOffset = new Vector3(0f, 6f, 0f);

        [SerializeField, Min(0.01f)] private float _openDuration = 1.5f;

        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private Rigidbody _body;
        private StageManager _stageManager;
        private Coroutine _moveRoutine;

        private void Awake()
        {
            _closedPosition = transform.position;
            _openPosition = _closedPosition + _openOffset;
            TryGetComponent(out _body);
        }

        private IEnumerator Start()
        {
            // Core systems are created by the gameplay manager, wait until the stage manager is available.
            while (GameplayCoreManager.Instance == null || GameplayCoreManager.Instance.StageManager == null)
            {
                yield return null;
            }

            _stageManager = GameplayCoreManager.Instance.StageManager;
            _stageManager.TheStageIsSet += OnStageSet;

            // Sync with the current state in case the stage was set before the subscription.
            ApplyState(_stageManager.CurrentStageIndex, instant: true);
        }

        private void OnDestroy()
        {
            if (_stageManager != null)
            {
                _stageManager.TheStageIsSet -= OnStageSet;
            }
        }

        private void OnStageSet(int stageIndex)
        {
            ApplyState(stageIndex, instant: false);
        }

        private void ApplyState(int stageIndex, bool instant)
        {
            var shouldBeOpen = stageIndex >= _openOnStageIndex;
            var target = shouldBeOpen ? _openPosition : _closedPosition;

            if (_moveRoutine != null)
            {
                StopCoroutine(_moveRoutine);
                _moveRoutine = null;
            }

            if (instant || !isActiveAndEnabled)
            {
                SetPosition(target);
                return;
            }

            _moveRoutine = StartCoroutine(MoveRoutine(target));
        }

        private IEnumerator MoveRoutine(Vector3 target)
        {
            var start = transform.position;
            var elapsed = 0f;

            while (elapsed < _openDuration)
            {
                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
                var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _openDuration));
                SetPosition(Vector3.Lerp(start, target, t));
            }

            SetPosition(target);
            _moveRoutine = null;
        }

        private void SetPosition(Vector3 position)
        {
            if (_body != null && _body.isKinematic)
            {
                _body.MovePosition(position);
                return;
            }

            transform.position = position;
        }
    }
}
