using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FrameWork.Helper;
using UnityEngine.SceneManagement;

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

