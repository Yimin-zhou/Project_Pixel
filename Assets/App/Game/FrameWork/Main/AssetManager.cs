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
        /// ����һ����Դ,��onFinish�ص��д�AsyncOperationHandle.Result�����Load������Object.
        /// identifier��һ����ǣ������ϣ���ͷ�һϵ�е���Load������Object,����Դ���ñ�ǣ�������ϣ����Load�����Ķ�������һ��������ʹ�ã�
        /// ������ж�ص�ʱ����ϣ�����Ƕ����ͷţ���ô���identifier���Դ����������֡�������ж�ص�ʱ�򶼻�ȥ�Զ��ͷš�
        /// �����û�д���identifier����ô�������Release����������ж�ز�����
        /// ������¡��Load������Assets����ô�����Լ��������¡������Assets���������ڣ���Assets���ͷ�ʱ����¡�����Ķ�����ܻ���ֶ�ʧ���õ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address">����string/AssetReference/AssetLabelReference</param>
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
        /// ͨ��IResourceLocation����һ����Դ���������ͨ��������Ҫ����һϵ�е���Դʱ������Ҫ������ص���Դ���й��˻�����֯��������ȵ���
        /// Addressables.LoadResourceLocationsAsyn����ȡIResourceLocation���б�Ȼ��ͨ�����Լ��ķ�ʽ�������ݹ��˺���֯���ٵ��ø÷��������м���
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
