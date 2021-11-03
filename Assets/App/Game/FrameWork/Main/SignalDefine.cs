using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FrameWork.Main
{
    public delegate void Signal_Test_Add(int a ,int b);

    public delegate int Signal_Test_Sub(int a,int b);

    public delegate void Signal_Test_Not(bool a);

    public delegate void Signal_Test_Debug(string a);
}
