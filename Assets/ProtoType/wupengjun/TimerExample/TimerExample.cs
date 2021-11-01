using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameWork.Main;
using FrameWork.Helper;

public class TimerTest : MonoBehaviour
{
    public TimerIdentifer timerID;

    public float currTime;

    void Start()
    {
        timerID = MonoSingleton<TimerManager>.Inst.AddTimer(3, 0, 3, null, null, () =>
        {
            Debug.LogError("hahaha");
        });
    }

    // Update is called once per frame
    void Update()
    {
        currTime += Time.deltaTime;
        if (currTime > 7)
        {
            MonoSingleton<TimerManager>.Inst.RemoveTimer(timerID);
            currTime = 0;
        }
    }
}
