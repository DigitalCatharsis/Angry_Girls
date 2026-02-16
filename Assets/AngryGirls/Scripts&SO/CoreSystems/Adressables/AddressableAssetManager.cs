using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Angry_Girls
{
    /// <summary>
    /// Interface for asset loading and management
    /// </summary>
    public interface IAssetProvider
    {
        UniTask<Sprite> LoadSpriteAsync(AssetReferenceT<Sprite> assetReference);
        UniTask<T> LoadScriptableObjectAsync<T>(AssetReferenceT<T> assetReference) where T : ScriptableObject;
        UniTask<Sprite> LoadSpriteAsync(string assetGuid);
        UniTask<T> LoadScriptableObjectAsync<T>(string assetGuid) where T : ScriptableObject;
        void ReleaseAsset(UnityEngine.Object asset);
        void ReleaseAssetByGuid(string guid);
        void ReleaseAll();
    }

    /// <summary>
    /// Manager for loading and caching addressable assets
    /// </summary>
    public class AddressableAssetManager : IAssetProvider, IDisposable
    {
        private abstract class TrackedAssetHandleBase
        {
            public string AssetGUID { get; protected set; }
            public abstract void Release();
            public abstract int ReferenceCount { get; protected set; }
            public abstract void AddReference();
            public abstract bool RemoveReference();
        }

        private class TrackedAssetHandle<T> : TrackedAssetHandleBase where T : UnityEngine.Object
        {
            public AsyncOperationHandle<T> Handle { get; private set; }
            public T Asset { get; private set; }
            public override int ReferenceCount { get; protected set; }

            public TrackedAssetHandle(AsyncOperationHandle<T> handle, string assetGuid)
            {
                this.Handle = handle;
                this.Asset = handle.Result;
                this.AssetGUID = assetGuid;
                this.ReferenceCount = 1;
            }

            public override void AddReference() => ReferenceCount++;

            public override bool RemoveReference()
            {
                ReferenceCount--;
                return ReferenceCount <= 0;
            }

            public override void Release()
            {
                Addressables.Release(Handle);
                Debug.Log($"AddressableAssetManager: Released handle for asset GUID: {AssetGUID}, Type: {typeof(T).Name}");
            }
        }

        private readonly Dictionary<string, TrackedAssetHandleBase> _trackedHandleBases = new();
        private readonly Dictionary<UnityEngine.Object, string> _assetToGuidMap = new();
        private readonly ConcurrentDictionary<string, AsyncLazy<UnityEngine.Object>> _loadingTasks = new();

        /// <summary>
        /// Load asset using AssetReference
        /// </summary>
        private async UniTask<T> LoadAssetAsync<T>(AssetReferenceT<T> assetReference) where T : UnityEngine.Object
        {
            return await LoadAssetAsync<T>(assetReference.AssetGUID);
        }

        /// <summary>
        /// Load asset using GUID
        /// </summary>
        private async UniTask<T> LoadAssetAsync<T>(string assetGuid) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetGuid))
            {

                Debug.LogWarning($"LoadAssetAsync<{typeof(T).Name}>: AssetGUID is empty.");
                return null;
            }

            AsyncLazy<UnityEngine.Object> asyncLazy;

            lock (_trackedHandleBases)
            {
                if (_trackedHandleBases.TryGetValue(assetGuid, out var handle) && handle is TrackedAssetHandle<T> th)
                {
                    th.AddReference();
                    return th.Asset;
                }

                var lazy = _loadingTasks.GetOrAdd(assetGuid, guid =>
                    new AsyncLazy<UnityEngine.Object>(async () =>
                    {
                        var handle = Addressables.LoadAssetAsync<T>(guid);
                        var asset = await handle.ToUniTask();

                        var tracked = new TrackedAssetHandle<T>(handle, guid);
                        lock (_trackedHandleBases)
                        {
                            _loadingTasks.TryRemove(assetGuid, out _);

                            if (!_trackedHandleBases.ContainsKey(guid))
                            {
                                _trackedHandleBases[guid] = tracked;
                                _assetToGuidMap[asset] = guid;
                            }
                            else
                            {
                                Addressables.Release(handle);
                                return ((TrackedAssetHandle<T>)_trackedHandleBases[guid]).Asset;
                            }
                        }
                        Debug.Log($"AddressableAssetManager: Loaded {guid}, Type: {typeof(T).Name}");
                        return asset;
                    })
                );
                asyncLazy = lazy;
            }

            try
            {
                var asset = await asyncLazy;
                return asset as T;
            }
            catch (Exception ex)
            {
                Debug.LogError($"LoadAssetAsync failed for {assetGuid}: {ex}");
                _loadingTasks.TryRemove(assetGuid, out _);
                return null;
            }
        }

        /// <inheritdoc/>
        public UniTask<Sprite> LoadSpriteAsync(AssetReferenceT<Sprite> assetReference)
            => LoadAssetAsync(assetReference);

        /// <inheritdoc/>
        public async UniTask<T> LoadScriptableObjectAsync<T>(AssetReferenceT<T> assetReference) where T : ScriptableObject
        {
            if (assetReference == null || string.IsNullOrEmpty(assetReference.AssetGUID))
            {
                Debug.LogWarning($"LoadScriptableObjectAsync<{typeof(T).Name}>: AssetReference is null or AssetGUID is empty.");
                return null;
            }

            return await LoadScriptableObjectAsync<T>(assetReference.AssetGUID);
        }

        public async UniTask<T> LoadScriptableObjectAsync<T>(string assetGuid) where T : ScriptableObject
        {
            var obj = await LoadAssetAsync<ScriptableObject>(assetGuid);

            if (obj is T typedObj)
            {
                return typedObj;
            }
            else if (obj != null)
            {
                Debug.LogError($"AddressableAssetManager: Loaded asset is not of type {typeof(T).Name}. GUID: {assetGuid}");
                ReleaseAssetByGuid(assetGuid);
                return null;
            }

            return null;
        }

        /// <inheritdoc/>
        public UniTask<Sprite> LoadSpriteAsync(string assetGuid) => LoadAssetAsync<Sprite>(assetGuid);

        /// <summary>
        /// Load GameObject by GUID
        /// </summary>
        public UniTask<GameObject> LoadGameObjectAsync(string assetGuid) => LoadAssetAsync<GameObject>(assetGuid);


        public void ReleaseAsset(UnityEngine.Object asset)
        {
            if (asset == null) return;

            if (_assetToGuidMap.TryGetValue(asset, out string guid))
            {
                ReleaseAssetByGuid(guid);
            }
            else
            {
                Debug.LogWarning($"AddressableAssetManager: Attempted to release an asset that is not tracked: {asset?.name ?? "null"}");
            }
        }

        /// <inheritdoc/>
        public void ReleaseAssetByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return;

            if (_trackedHandleBases.TryGetValue(guid, out TrackedAssetHandleBase handle))
            {
                if (handle.RemoveReference())
                {
                    _trackedHandleBases.Remove(guid);

                    if (handle is TrackedAssetHandle<UnityEngine.Object> typedHandle)
                    {
                        _assetToGuidMap.Remove(typedHandle.Asset);
                    }

                    handle.Release();
                    _loadingTasks.TryRemove(guid, out _);
                }
                else
                {
                    Debug.Log($"AddressableAssetManager: Decremented reference count for {guid}: {handle.ReferenceCount}");
                }
            }
            else
            {
                Debug.LogWarning($"AddressableAssetManager: Attempted to release an untracked asset with GUID: {guid}");
            }
        }

        /// <inheritdoc/>
        public void ReleaseAll()
        {
            Debug.Log($"AddressableAssetManager: Releasing {_trackedHandleBases.Count} tracked assets...");

            var guidsToRelease = new List<string>(_trackedHandleBases.Keys);

            foreach (string guid in guidsToRelease)
            {
                if (_trackedHandleBases.TryGetValue(guid, out var handle))
                {
                    _trackedHandleBases.Remove(guid);

                    if (handle is TrackedAssetHandle<UnityEngine.Object> typedHandle)
                    {
                        _assetToGuidMap.Remove(typedHandle.Asset);
                    }

                    handle.Release();
                }
            }

            _loadingTasks.Clear();

            Debug.Log("AddressableAssetManager: All tracked assets released.");
        }

        /// <summary>
        /// Get AssetGUID associated with a loaded object
        /// </summary>
        public string GetAssetGuid(UnityEngine.Object asset)
        {
            if (asset == null) throw new Exception ("Addressables AssetManager: Asset is null");

            if (_assetToGuidMap.TryGetValue(asset, out string guid))
            {
                return guid;
            }

            Debug.Log("Addressables AssetManager: CANT GET GUI FOR ASSET" +  asset.name);
            return null;
        }

        /// <summary>
        /// Get current reference count for an asset
        /// </summary>
        public int GetReferenceCount(string guid)
        {
            if (_trackedHandleBases.TryGetValue(guid, out var handle))
            {
                return handle.ReferenceCount;
            }
            return 0;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ReleaseAll();
            Debug.Log("AddressableAssetManager: Disposed");
        }
    }
}