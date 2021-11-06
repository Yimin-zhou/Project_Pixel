using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace FrameWork.Main
{
    //test
    public delegate void Signal_Test_Add(int a ,int b);

    public delegate int Signal_Test_Sub(int a,int b);

    public delegate void Signal_Test_Not(bool a);

    public delegate void Signal_Test_Debug(string a);

    //formal
    public delegate void Signal_Load_Scene(string sceneName, LoadSceneMode mode);

    public delegate void Signal_Unload_Scene(string sceneName);
}
