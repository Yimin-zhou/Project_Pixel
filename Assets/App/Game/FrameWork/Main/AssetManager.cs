using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using static UnityEngine.AddressableAssets.Addressables;

namespace FrameWork.Main
{
    public class AssetManager : IDisposable
    {
        private Dictionary<string, List<UnityEngine.Object>> _loadedAssetsDic = new Dictionary<string, List<UnityEngine.Object>>();

        /// <summary>
        /// 加载一个资源,在onFinish回调中从AsyncOperationHandle.Result获得你Load出来的Object.
        /// identifier是一个标记，如果你希望释放一系列的你Load出来的Object,你可以传入该标记，比如你希望你Load出来的东西都在一个场景中使用，
        /// 当场景卸载的时候你希望它们都被释放，那么这个identifier可以传场景的名字。场景在卸载的时候都会去自动释放。
        /// 如果你没有传入identifier，那么请你调用Release函数来进行卸载操作。
        /// 如果你克隆了Load出来的Assets，那么请你自己管理你克隆出来的Assets的生命周期，当Assets被释放时，克隆出来的对象可能会出现丢失引用的情况
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address">传入string/AssetReference/AssetLabelReference</param>
        /// <param name="onFinish"></param>
        /// <param name="identifier"></param>
        public void LoadAsset<T>(object address, Action<AsyncOperationHandle<T>> onFinish, string identifier = null) where T : UnityEngine.Object
        {
            if (address == null)
            {
                Debug.LogError("assetRef is null!!!");
                return;
            }

            var op = Addressables.LoadAssetAsync<T>(address);
            op.Completed += onFinish;
            if (!string.IsNullOrEmpty(identifier))
            {
                op.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded && op.IsDone)
                    {
                        List<UnityEngine.Object> list;
                        if (_loadedAssetsDic.TryGetValue(identifier, out list))
                        {
                            foreach (var asset in list)
                            {
                                if (asset == op.Result)
                                {
                                    Debug.LogError("repeated assets loaded!,address:" + address);
                                    return;
                                }
                            }
                            list.Add(op.Result);
                        }
                        else
                        {
                            list = new List<UnityEngine.Object>();
                            list.Add(op.Result);
                            _loadedAssetsDic.Add(identifier, list);
                        }
                    }
                };
            }
        }

        /// <summary>
        /// 通过IResourceLocation加载一个资源，这个函数通常用于你要加载一系列的资源时，但你要对你加载的资源进行过滤或者组织，你可以先调用
        /// Addressables.LoadResourceLocationsAsyn来获取IResourceLocation的列表，然后通过你自己的方式进行数据过滤和组织，再调用该方法来进行加载
        /// </summary>
        private void LoadAsset<T>(IResourceLocation address, Action<AsyncOperationHandle<T>> onFinish, string identifier = null) where T : UnityEngine.Object
        {
            if (address == null)
            {
                Debug.LogError("assetRef is null!!!");
                return;
            }

            var op = Addressables.LoadAssetAsync<T>(address);
            op.Completed += onFinish;
            if (!string.IsNullOrEmpty(identifier))
            {
                op.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded && op.IsDone)
                    {
                        List<UnityEngine.Object> list;
                        if (_loadedAssetsDic.TryGetValue(identifier, out list))
                        {
                            foreach (var asset in list)
                            {
                                if (asset == op.Result)
                                {
                                    Debug.LogError("repeated assets loaded!,address:" + address);
                                    return;
                                }
                            }
                            list.Add(op.Result);
                        }
                        else
                        {
                            list = new List<UnityEngine.Object>();
                            list.Add(op.Result);
                            _loadedAssetsDic.Add(identifier, list);
                        }
                    }
                };
            }
        }

        public void LoadAssetsBySingleKey<T>(object key, Action<T> onPerAssetLoaded,string identifier = null) where T : UnityEngine.Object
        {
            if (key == null)
            {
                Debug.LogError("assetRef is null!!!");
                return;
            }

            var op = Addressables.LoadAssetsAsync<T>(key, onPerAssetLoaded,true);
            if (!string.IsNullOrEmpty(identifier))
            {
                op.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded && op.IsDone)
                    {
                        List<UnityEngine.Object> list;
                        if (_loadedAssetsDic.TryGetValue(identifier, out list))
                        {
                            foreach (var asset in list)
                            {
                                foreach(var loadAsset in op.Result)
                                {
                                    if (asset == loadAsset)
                                    {
                                        Debug.LogError("repeated assets loaded!,address:" + key);
                                        return;
                                    }
                                }

                            }
                            list.AddRange(op.Result);
                        }
                        else
                        {
                            list = new List<UnityEngine.Object>();
                            list.AddRange(op.Result);
                            _loadedAssetsDic.Add(identifier, list);
                        }
                    }
                };
            }
        }

        public void LoadAssetsByKeys<T>(IEnumerable keys, Action<T> onPerAssetLoaded, MergeMode mode, string identifier = null) where T : UnityEngine.Object
        {
            if (keys == null)
            {
                Debug.LogError("assetRef is null!!!");
                return;
            }

            var op = Addressables.LoadAssetsAsync<T>(keys, onPerAssetLoaded, mode, true);
            if (!string.IsNullOrEmpty(identifier))
            {
                op.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded && op.IsDone)
                    {
                        List<UnityEngine.Object> list;
                        if (_loadedAssetsDic.TryGetValue(identifier, out list))
                        {
                            foreach (var asset in list)
                            {
                                foreach (var loadAsset in op.Result)
                                {
                                    if (asset == loadAsset)
                                    {
                                        Debug.LogError("repeated assets loaded!,address:" + keys);
                                        return;
                                    }
                                }

                            }
                            list.AddRange(op.Result);
                        }
                        else
                        {
                            list = new List<UnityEngine.Object>();
                            list.AddRange(op.Result);
                            _loadedAssetsDic.Add(identifier, list);
                        }
                    }
                };
            }
        }

        public void Release<T>(T unuseAsset) where T : UnityEngine.Object
        {
            if (unuseAsset == null)
            {
                Debug.LogError("assetRef is null!!!");
                return;
            }

            foreach (var item in _loadedAssetsDic)
            {
                foreach (var asset in item.Value)
                {
                    if (asset == unuseAsset)
                    {
                        item.Value.Remove(asset);
                    }
                }
            }
            Addressables.Release(unuseAsset);
        }

        public void ReleaseAssetsByIdentifier(string id)
        {
            if (_loadedAssetsDic.ContainsKey(id))
            {
                foreach (var asset in _loadedAssetsDic[id])
                {
                    Addressables.Release(asset);
                }
                _loadedAssetsDic.Remove(id);
            }
        }
        
        public void ReleaseAllSceneAsset()
        {
            foreach (var item in _loadedAssetsDic)
            {
                foreach (var asset in item.Value)
                {
                    Addressables.Release(asset);
                }
            }
            _loadedAssetsDic.Clear();
        }

        public void Dispose()
        {
            foreach (var item in _loadedAssetsDic)
            {
                foreach (var asset in item.Value)
                {
                    Addressables.Release(asset);
                }
            }
            _loadedAssetsDic = null;
        }
    }
}
