using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick; // 실행 시간
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            return other.execTick - execTick;
        }
    }
    //[1000ms][1200ms][1300ms][1330ms][1340ms]같이 먼 큐는 따로 관리하고
    //[20ms][20ms][20ms][20ms][20ms]임박한 큐를 수거해서 따로 관리하는 방안도 있음
    class JobTimer
    {
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem> ();
        object _lock = new object ();

        public static JobTimer Instance { get; } = new JobTimer ();

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElem job;
            job.execTick = System.Environment.TickCount+tickAfter;
            job.action = action;

            lock (_lock)
            {
                _pq.Push (job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;
                
                JobTimerElem job;

                lock (_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    job = _pq.Peek();
                    if (job.execTick > now)
                        break;

                    _pq.Pop();
                }

                job.action.Invoke ();
            }
        }
    }
}
