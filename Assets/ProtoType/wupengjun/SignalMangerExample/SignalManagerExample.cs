using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameWork.Helper;
using FrameWork.Main;

public class SignalManagerExample : MonoBehaviour
{
    float time;
    int count;
    void Start()
    {
        Singleton<SignalManager>.Inst.Subscribe<Signal_Test_Add>(Add1);
        Singleton<SignalManager>.Inst.Subscribe<Signal_Test_Add>(Add2);
        Singleton<SignalManager>.Inst.Subscribe<Signal_Test_Not>(Not);
    }

    public void Add1(int a, int b)
    {
        int s = a + b;
        Debug.Log("Add1:" + s);
    }

    public void Add2(int a, int b)
    {
        int s = a * a + b * b;
        Debug.Log("Add2:" + s);
    }

    public void Not(bool a)
    {
        a = !a;
        Debug.Log("Not:" + a);
    }


    void Update()
    {
        time += Time.deltaTime;
        if (time > 3)
        {
            count += 1;
            time = 0;
            Singleton<SignalManager>.Inst.Rasie<Signal_Test_Add>(count, count);

        }
        Singleton<SignalManager>.Inst.Rasie<Signal_Test_Not>(false);
    }
}
