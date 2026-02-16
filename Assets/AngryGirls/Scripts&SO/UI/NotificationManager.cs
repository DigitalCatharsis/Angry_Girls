using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Manages display of notifications to the player.
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        [Header("Notification UI")]
        [SerializeField] private GameObject _notificationPrefab;
        [SerializeField] private Transform _notificationContainer;
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1280, 720);
        [SerializeField] private float _defaultDuration = 0.25f;

        private Queue<Notification> _notificationQueue = new Queue<Notification>();
        private bool _isShowingNotification = false;

        private void Awake()
        {
            InitializeContainer();
        }

        private void InitializeContainer()
        {
            if (_notificationContainer == null)
            {
                Canvas existingCanvas = FindObjectOfType<Canvas>();
                if (existingCanvas != null)
                {
                    _notificationContainer = existingCanvas.transform;
                }
                else
                {
                    CreateDefaultCanvas();
                }
            }
        }

        private void CreateDefaultCanvas()
        {
            var canvasGO = new GameObject("NotificationCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _referenceResolution;

            canvasGO.AddComponent<GraphicRaycaster>();

            _notificationContainer = canvas.transform;

            Debug.Log($"Created default canvas with resolution: {_referenceResolution}");
        }

        /// <summary>
        /// Show a notification message.
        /// </summary>
        public void ShowNotification(string message, float duration = -1f)
        {
            if (_notificationContainer == null)
            {
                InitializeContainer();
            }

            if (_notificationPrefab == null)
            {
                Debug.LogWarning("Notification prefab is not assigned!");
                return;
            }

            if (duration <= 0) duration = _defaultDuration;

            var notification = new Notification(message, duration);
            _notificationQueue.Enqueue(notification);

            if (!_isShowingNotification)
            {
                ShowNextNotification().Forget();
            }
        }

        private async UniTaskVoid ShowNextNotification()
        {
            _isShowingNotification = true;

            try
            {
                //TODO: перекурить токены
                var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    this.GetCancellationTokenOnDestroy()
                );

                SceneManager.activeSceneChanged += CancelOnSceneChange;

                void CancelOnSceneChange(Scene current, Scene next)
                {
                    linkedTokenSource.Cancel();
                }

                while (_notificationQueue.Count > 0 && !linkedTokenSource.IsCancellationRequested)
                {
                    var notification = _notificationQueue.Dequeue();
                    await DisplayNotificationAsync(notification, linkedTokenSource.Token);
                }

                SceneManager.activeSceneChanged -= CancelOnSceneChange;
                linkedTokenSource.Dispose();
            }
            catch (System.OperationCanceledException)
            {
            }
            finally
            {
                _isShowingNotification = false;
            }
        }

        private async UniTask DisplayNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            if (_notificationPrefab == null || _notificationContainer == null || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var notificationGO = Instantiate(_notificationPrefab, _notificationContainer);

            var notificationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            notificationTokenSource.Token.Register(() =>
            {
                if (notificationGO != null)
                {
                    UnityEngine.Object.Destroy(notificationGO);
                }
            });

            var textComponent = notificationGO.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent != null)
            {
                textComponent.text = notification.Message;
            }

            notificationGO.SetActive(true);

            try
            {
                await UniTask.Delay((int)(notification.Duration * 1000),
                    cancellationToken: notificationTokenSource.Token);

                // Анимация исчезновения
                var canvasGroup = notificationGO.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    float fadeTime = 0.5f;
                    float elapsedTime = 0f;

                    while (elapsedTime < fadeTime && !notificationTokenSource.Token.IsCancellationRequested)
                    {
                        elapsedTime += Time.deltaTime;
                        canvasGroup.alpha = 1f - (elapsedTime / fadeTime);
                        await UniTask.Yield(cancellationToken: notificationTokenSource.Token);
                    }
                }
            }
            catch (System.OperationCanceledException)
            {
                notificationTokenSource.Dispose();
                return;
            }

            notificationTokenSource.Dispose();

            if (notificationGO != null)
            {
                UnityEngine.Object.Destroy(notificationGO);
            }
        }

        private class Notification
        {
            public string Message { get; }
            public float Duration { get; }

            public Notification(string message, float duration)
            {
                Message = message;
                Duration = duration;
            }
        }
    }
}