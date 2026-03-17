using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Angry_Girls
{
    public class ConfirmationManager : MonoBehaviour
    {
        [Header("Confirmation UI")]
        [SerializeField] private GameObject _confirmationPrefab;
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1280, 720);

        private Queue<Func<UniTask>> _confirmationQueue = new Queue<Func<UniTask>>();
        private bool _isShowingConfirmation = false;

        private void Awake()
        {
            InitializeContainer();
        }

        private void InitializeContainer()
        {
            CreateDefaultCanvas();
        }

        private void CreateDefaultCanvas()
        {
            var canvasGO = new GameObject("ConfirmationCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _referenceResolution;

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// Show a confirmation dialog and get the result asynchronously.
        /// </summary>
        public UniTask<bool> ShowConfirmationAsync(string message, UnityAction yesAction, UnityAction noAction)
        {
            if (_confirmationPrefab == null)
            {
                Debug.LogError("Confirmation prefab is not assigned!");
                return UniTask.FromResult(false);
            }

            var completionSource = new UniTaskCompletionSource<bool>();

            _confirmationQueue.Enqueue(async () =>
            {
                using (var cts = new CancellationTokenSource())
                {
                    SceneManager.activeSceneChanged += CancelOnSceneChange;
                    void CancelOnSceneChange(Scene current, Scene next) => cts.Cancel();

                    try
                    {
                        bool result = await ShowConfirmationInternalAsync(message, yesAction, noAction, cts.Token);
                        completionSource.TrySetResult(result);
                    }
                    catch (OperationCanceledException)
                    {
                        completionSource.TrySetCanceled();
                    }
                    finally
                    {
                        SceneManager.activeSceneChanged -= CancelOnSceneChange;
                    }
                }
            });

            if (!_isShowingConfirmation)
                ProcessQueue().Forget();

            return completionSource.Task;
        }

        private async UniTaskVoid ProcessQueue()
        {
            _isShowingConfirmation = true;
            while (_confirmationQueue.Count > 0)
            {
                var confirmationTask = _confirmationQueue.Dequeue();
                await confirmationTask();
            }
            _isShowingConfirmation = false;
        }

        private async UniTask<bool> ShowConfirmationInternalAsync(string message, UnityAction yesAction, UnityAction noAction, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            var confirmationGO = Instantiate(_confirmationPrefab);
            confirmationGO.transform.SetParent(this.gameObject.transform, false);
            confirmationGO.SetActive(true);

            var panel = confirmationGO.GetComponentInChildren<ConfirmationPanel>();
            if (panel == null)
            {
                Debug.LogError("Confirmation prefab must have a ConfirmationPanel component.");
                Destroy(confirmationGO);
                return false;
            }

            if (panel.MessageText != null)
                panel.MessageText.text = message;
            else
                Debug.LogWarning("MessageText is not assigned in ConfirmationPanel.");

            var yesButton = panel.YesButton;
            var noButton = panel.NoButton;

            if (yesButton == null || noButton == null)
            {
                Debug.LogError("YesButton and NoButton must be assigned in ConfirmationPanel.");
                Destroy(confirmationGO);
                return false;
            }

            if (yesAction != null) yesButton.onClick.AddListener(yesAction);
            if (noAction != null) noButton.onClick.AddListener(noAction);

            var tcs = new UniTaskCompletionSource<bool>();

            void OnYes() => tcs.TrySetResult(true);
            void OnNo() => tcs.TrySetResult(false);

            yesButton.onClick.AddListener(OnYes);
            noButton.onClick.AddListener(OnNo);

            cancellationToken.Register(() => tcs.TrySetCanceled());

            bool result;
            try
            {
                result = await tcs.Task;
            }
            finally
            {
                yesButton.onClick.RemoveAllListeners();
                noButton.onClick.RemoveAllListeners();

                if (confirmationGO != null)
                    Destroy(confirmationGO);
            }

            return result;
        }
    }
}