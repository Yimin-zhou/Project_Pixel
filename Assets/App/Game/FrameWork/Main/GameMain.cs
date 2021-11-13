using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FrameWork.Helper;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

namespace FrameWork.Main
{
    public class GameMain : MonoBehaviour
    {
        [ReadOnly]
        public GameObject managerRoot;
        [ReadOnly]
        public Camera MainCamera;

        void Start()
        {
            InitBaseService();
            Singleton<GameSceneManager>.Inst.LoadSceneAsync("TestScene2", LoadSceneMode.Single);

            //Texture2D tx = null;
            //Material mat = null;
            //Singleton<AssetManager>.Inst.LoadAsset<Material>("main_character_M", (op) =>
            //{
            //    if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && op.IsDone)
            //    {
            //        mat = op.Result;
            //        Debug.LogError(op.Result);
            //    }
            //});
            //Singleton<AssetManager>.Inst.LoadAsset<Material>("main_character_M", (op) =>
            //{
            //    if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && op.IsDone)
            //    {
            //        Debug.LogError(op.Result);
            //    }
            //});
            //Singleton<AssetManager>.Inst.LoadAsset<Texture2D>("fox_palette", (op) =>
            //{
            //    if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && op.IsDone)
            //    {
            //        tx = op.Result;
            //        Debug.LogError(op.Result);
            //    }
            //});
            //MonoSingleton<TimerManager>.Inst.AddTimer(3, 0, 0, null, null, () =>
            //{
            //    Singleton<AssetManager>.Inst.Release<Material>(mat);
            //});

            //MonoSingleton<TimerManager>.Inst.AddTimer(4, 0, 0, null, null, () =>
            //{
            //    Singleton<GameSceneManager>.Inst.UnloadSceneAsync("TestScene2");
            //});
        }

        private void InitBaseService()
        {
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(managerRoot);
        }

        void Update()
        {

        }
    }
}

