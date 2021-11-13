using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FrameWork.Helper;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace FrameWork.Main
{
    public class GameSceneManager : IDisposable
    {
        public AsyncOperationHandle<SceneInstance> loadOp;
        public AsyncOperationHandle<SceneInstance> unloadOp;

        private Dictionary<string, SceneInstance> _loadedSceneDic = new Dictionary<string, SceneInstance>();

        public void Dispose()
        {
            _loadedSceneDic.Clear();
            _loadedSceneDic = null;
        }

        //不开放使用AssetReference为参数的原因是不好获取场景名字，其中有两个字段asset/editorAsset在editor模式下和player模式下不被定义
        public void LoadSceneAsync(string sceneName, LoadSceneMode mode, Action<AsyncOperationHandle<SceneInstance>> onFinish = null)
        {
            MonoSingleton<AsyncManager>.Inst.RunCoroutine(LoadSceneTask(sceneName, mode, onFinish));
        }

        public void UnloadSceneAsync(string sceneName, Action<AsyncOperationHandle<SceneInstance>> onFinish = null)
        {
            if (!_loadedSceneDic.ContainsKey(sceneName))
            {
                Debug.LogError("scene doesnt exist!sceneName:" + sceneName);
                return;
            }
            if (SceneManager.sceneCount == 1)
            {
                Debug.LogError("there is only one scene,cant unload!");
                return;
            }
            MonoSingleton<AsyncManager>.Inst.RunCoroutine(UnloadSceneTask(sceneName, onFinish));
        }

        public void UnloadScenesExcept(string[] excepts)
        {
            foreach (var item in _loadedSceneDic)
            {
                SceneInstance scene = item.Value;
                bool isUnload = true;
                foreach (var except in excepts)
                {
                    if (except == scene.Scene.name)
                    {
                        isUnload = false;
                    }
                }
                if (isUnload)
                {
                    UnloadSceneTask(scene.Scene.name);
                }
            }
        }

        private IEnumerator LoadSceneTask(string sceneName, LoadSceneMode mode, Action<AsyncOperationHandle<SceneInstance>> onFinish = null)
        {
            if (mode == LoadSceneMode.Single)
            {
                Singleton<AssetManager>.Inst.ReleaseAllSceneAsset();
            }
            loadOp = Addressables.LoadSceneAsync(sceneName, mode);
            if (onFinish != null)
                loadOp.Completed += onFinish;
            yield return loadOp;
            if (loadOp.IsDone && loadOp.Status == AsyncOperationStatus.Succeeded)
            {
                var sceneIns = loadOp.Result;
                if (_loadedSceneDic.ContainsKey(sceneName))
                    _loadedSceneDic[sceneName] = sceneIns;
                else
                    _loadedSceneDic.Add(sceneName, sceneIns);
                Singleton<SignalManager>.Inst.Rasie<Signal_Unload_Scene>(sceneName);
            }
            else
            {
                Debug.LogError("Load Scene Failed!,sceneName:" + sceneName);
            }
        }

        private IEnumerator UnloadSceneTask(string sceneName, Action<AsyncOperationHandle<SceneInstance>> onFinish = null)
        {
            Singleton<AssetManager>.Inst.ReleaseAssetsByIdentifier(sceneName);
            unloadOp = Addressables.UnloadSceneAsync(_loadedSceneDic[sceneName]);
            if (onFinish != null)
                unloadOp.Completed += onFinish;
            yield return unloadOp;
            _loadedSceneDic.Remove(sceneName);
            Singleton<SignalManager>.Inst.Rasie<Signal_Unload_Scene>(sceneName);
        }
    }
}

