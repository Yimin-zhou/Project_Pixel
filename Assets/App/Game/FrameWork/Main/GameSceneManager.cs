using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FrameWork.Helper;
using System;

namespace FrameWork.Main
{
    public class GameSceneManager:IDisposable
    {
        public AsyncOperation loadOp;
        public AsyncOperation unloadOp;

        public void Dispose()
        {
            loadOp = null;
            unloadOp = null;
        }

        /// <summary>
        /// 如果你要访问AsyncOperation中的属性就用AsyncOperation来引用该函数的返回值
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public void LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            int len = SceneManager.sceneCountInBuildSettings;
            bool hasScene = false;
            for (int i = 0; i < len; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName)
                {
                    hasScene = true;
                    break;
                }
            }
            if (hasScene)
            {
                MonoSingleton<AsyncManager>.Inst.RunCoroutine(LoadSceneTask(sceneName, mode));
            }
        }

        public void UnloadSceneAsync(string sceneName)
        {

            int len = SceneManager.sceneCount;
            bool hasScene = false;
            for (int i = 0; i < len; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    hasScene = true;
                    break;
                }
            }
            if (hasScene)
            {
                MonoSingleton<AsyncManager>.Inst.RunCoroutine(UnloadSceneTask(sceneName));
            }
        }

        public void UnloadScenesExcept(string[] excepts)
        {
            int len = SceneManager.sceneCount;
            for (int i = 0; i < len; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                bool isUnload = true;
                foreach (var except in excepts)
                {
                    if (except == scene.name)
                    {
                        isUnload = false;
                    }
                }
                if (isUnload)
                {
                    UnloadSceneAsync(scene.name);
                }
            }
        }

        private IEnumerator LoadSceneTask(string sceneName, LoadSceneMode mode)
        {
            loadOp = SceneManager.LoadSceneAsync(sceneName, mode);
            yield return loadOp;
            if (loadOp.isDone)
            {
                Singleton<SignalManager>.Inst.Rasie<Signal_Load_Scene>(sceneName, mode);
                loadOp = null;
            }
        }

        private IEnumerator UnloadSceneTask(string sceneName)
        {
            unloadOp = SceneManager.UnloadSceneAsync(sceneName);
            yield return unloadOp;
            if (unloadOp.isDone)
            {
                Singleton<SignalManager>.Inst.Rasie<Signal_Unload_Scene>(sceneName);
                unloadOp = null;
            }
        }
    }
}

