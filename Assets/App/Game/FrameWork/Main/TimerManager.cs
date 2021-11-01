using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace FrameWork.Main
{
    //参数为float time，该timer所剩余的时间
    public delegate void TimeChangeCallBack(float time);

    public delegate void TimerEndCallBack();

    public delegate void TimerStartCallBack();

    public struct TimerIdentifer
    {
        public int id;

        public TimerIdentifer(int id)
        {
            this.id = id;
        } 
    }

    public class TimerManager : MonoBehaviour
    {
        //已经发放的id的最大值
        private int _allocID = 0;
        //保存所有Timer的列表
        private Dictionary<int,Timer> _allTimer = new Dictionary<int, Timer>();
        //保存Timer的所有下标
        private List<int> _timerIndexes = new List<int>();

        /// <summary>
        /// 添加一个Timer，在不需要时请调用TimerManager.RemoveTimer来释放
        /// </summary>
        /// <param name="totalTime">timer的总时间</param>
        /// <param name="delayTime"></param>
        /// <param name="loopTime"></param>
        /// <returns></returns>
        public TimerIdentifer AddTimer(float totalTime, float totalDelay, int loopTime, TimerStartCallBack timeStart, TimeChangeCallBack timeChange, TimerEndCallBack timeEnd)
        {
            Timer timer = new Timer(totalTime, totalDelay, loopTime);
            if (timeStart != null)
                timer.timeStart += timeStart;
            if (timeChange != null)
                timer.timeChange += timeChange;
            if (timeEnd != null)
                timer.timeEnd += timeEnd;
            _allTimer.Add(_allocID, timer);
            _timerIndexes.Add(_allocID);
            timer.id = _allocID;
            _allocID++;
            timer.Start();
            TimerIdentifer identifer = new TimerIdentifer(timer.id);
            return identifer;
        }

        public void AddTimeStart(TimerIdentifer identifer, TimerStartCallBack timeStart)
        {
            Timer timer;
            if (_allTimer.TryGetValue(identifer.id, out timer))
            {
                if (timer != null && timeStart != null)
                    timer.timeStart += timeStart;
            }
        }

        public void AddTimeChange(TimerIdentifer identifer, TimeChangeCallBack timeChange)
        {
            Timer timer;
            if (_allTimer.TryGetValue(identifer.id, out timer))
            {
                if (timer != null && timeChange != null)
                    timer.timeChange += timeChange;
            }
        }

        public void AddTimeEnd(TimerIdentifer identifer, TimerEndCallBack timeEnd)
        {
            Timer timer;
            if (_allTimer.TryGetValue(identifer.id, out timer))
            {
                if (timer != null && timeEnd != null)
                    timer.timeEnd += timeEnd;
            }
        }

        public void RemoveTimer(TimerIdentifer identifer)
        {
            if (_allTimer.ContainsKey(identifer.id))
            {
                _allTimer.Remove(identifer.id);
                _timerIndexes.Remove(identifer.id);
            }
        }

        private void RemoveTimer(Timer timer)
        {
            if (_allTimer.ContainsKey(timer.id))
            {
                _allTimer.Remove(timer.id);
                _timerIndexes.Remove(timer.id);
            }
        }

        public void Pause(TimerIdentifer identifer)
        {
            Timer timer;
            if (_allTimer.TryGetValue(identifer.id, out timer))
            {
                if (timer != null)
                    timer.Pause();
            }
        }

        public void Continue(TimerIdentifer identifer)
        {
            Timer timer;
            if (_allTimer.TryGetValue(identifer.id, out timer))
            {
                if (timer != null)
                    timer.Continue();
            }
        }

        public bool IsLeft(TimerIdentifer identifer)
        {
            Timer timer;
            if (_allTimer.TryGetValue(identifer.id, out timer))
            {
                if (timer != null)
                    return timer.leftTime > 0;
            }
            Debug.LogError("cant find Timer by id:" + identifer.id);
            return false;
        }

        void Update()
        {
            for (int i= _timerIndexes.Count-1; i>=0; i--)
            {
                int index = _timerIndexes[i];
                Timer timer = _allTimer[index];
                //再一次判空是为了防止在改循环中timer被remove
                if (timer == null)
                {
                    continue;
                }
                if (timer.enable)
                {
                    timer.Update();
                }
                if (timer.isFinished())
                {
                    timer.Dispose();
                    RemoveTimer(timer);
                }
            }
        }

        private void OnDestroy()
        {
            for (int i = _timerIndexes.Count - 1; i >= 0; i--)
            {
                int index = _timerIndexes[i];
                Timer timer = _allTimer[index];
                timer.Dispose();
                RemoveTimer(timer);
            }
            _allocID = 0;
            _timerIndexes = null;
        }

        /// <summary>
        /// 对于要延迟一段时间后调用一次方法的设置delay=0，直接设置totalTime和loopTime,然后再设置timeEnd即可
        /// 对于要延迟一段时间后在一段时间内调用方法的，将delay设置为延迟时间，然后设置timeChange
        /// </summary>
        private class Timer : IDisposable
        {
            //参数为float time，该timer所剩余的时间
            private TimeChangeCallBack _timeChange;
            private TimerEndCallBack _timeEnd;
            private TimerStartCallBack _timeStart;

            public float totalTime;
            public float leftTime;
            public float totalDelay;
            //如果delay>0，则timeStart和后面的一系列操作都会在delay完后调用
            public float delay;
            public int id;
            //为false则不会帧调用update函数
            public bool enable;
            public bool isDelaying;
            //-1表示无限循环，直到被timeManager remove
            public int loopTime;

            public event TimeChangeCallBack timeChange { add { _timeChange += value; } remove { _timeChange -= value; } }
            public event TimerEndCallBack timeEnd { add { _timeEnd += value; } remove { _timeEnd -= value; } }
            public event TimerStartCallBack timeStart { add { _timeStart += value; } remove { _timeStart -= value; } }

            public Timer(float totalTime, float totalDelay, int loopTime)
            {
                this.totalTime = totalTime;
                this.leftTime = totalTime;
                this.totalDelay = totalDelay;
                this.delay = totalDelay;
                this.enable = true;
                if (loopTime == 0)
                    loopTime = 1;
                this.loopTime = loopTime;
                if (totalDelay > 0)
                    isDelaying = true;
            }

            /// <summary>
            /// 开始计时，但要在delay完之后才会调用timeStart和timeChange
            /// </summary>
            public void Start()
            {
                if (!isDelaying)
                    _timeStart?.Invoke();
            }

            public void Loop()
            {
                this.leftTime = totalTime;
                this.delay = totalDelay;
                this.enable = true;
                if (totalDelay > 0)
                    isDelaying = true;
                else
                    _timeStart?.Invoke();
            }

            /// <summary>
            /// 更新Timer
            /// </summary>
            public void Update()
            {
                if (leftTime <= 0)
                {
                    loopTime--;
                    _timeEnd?.Invoke();
                    enable = false;
                    if (loopTime != 0)
                    {
                        Loop();
                    }
                    return;
                }
                if (delay > 0)
                {
                    delay -= Time.deltaTime;
                    return;
                }
                else if (isDelaying)
                {
                    _timeStart?.Invoke();
                    isDelaying = false;
                    return;
                }
                leftTime -= Time.deltaTime;
                if (leftTime < 0)
                    leftTime = 0;
                _timeChange?.Invoke(leftTime);
            }

            /// <summary>
            /// 暂停,如还在delay期间就不管
            /// </summary>
            public void Pause()
            {
                if (delay > 0)
                    return;
                enable = false;
            }

            /// <summary>
            /// 继续
            /// </summary>
            public void Continue()
            {
                if (delay > 0)
                    return;
                enable = true;
            }

            //如果loopTime==0,则表示该Timer生命周期结束，需要被remove
            public bool isFinished()
            {
                return loopTime == 0;
            }

            public void Dispose()
            {
                _timeChange = null;
                _timeEnd = null;
                _timeStart = null;
            }
        }
    }

}
