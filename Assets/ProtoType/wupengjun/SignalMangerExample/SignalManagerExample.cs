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
        Singleton<SignalManager>.Inst.Subscribe<Signal_Test_Sub>(Add3);
        Singleton<SignalManager>.Inst.Subscribe<Signal_Test_Sub>(Add4);
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

    public int Add3(int a, int b)
    {
        Debug.Log(gameObject.name);
        int s = a + b;
        Debug.Log("Add3:" + s);
        return s;
    }

    public int Add4(int a, int b)
    {
        Debug.Log(gameObject.name);
        int s = a * a + b * b;
        Debug.Log("Add4:" + s);
        return s;
    }

    private void OnDestroy()
    {
        Singleton<SignalManager>.Inst.UnSubscibe<Signal_Test_Add>(Add1);
        Singleton<SignalManager>.Inst.UnSubscibe<Signal_Test_Add>(Add2);
        Singleton<SignalManager>.Inst.UnSubscibe<Signal_Test_Not>(Not);
        Singleton<SignalManager>.Inst.UnSubscibe<Signal_Test_Sub>(Add3);
        Singleton<SignalManager>.Inst.UnSubscibe<Signal_Test_Sub>(Add4);
    }


    void Update()
    {
        //time += Time.deltaTime;
        //if (time > 3)
        //{
        //    count += 1;
        //    time = 0;
        //    Singleton<SignalManager>.Inst.Rasie<Signal_Test_Add>(count, count);
        //}

        time += Time.deltaTime;
        if (time > 3)
        {
            count += 1;
            time = 0;
            object[] returns = Singleton<SignalManager>.Inst.Rasie<Signal_Test_Sub>(count, count);
            int sub = 0;
            foreach (var ret in returns)
            {
                int realRet = (int)ret;
                sub += realRet;
            }
            Debug.Log(sub);
        }
    }
}
