using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    //커맨드패턴의 예제이자 잡큐의 인터페이스
    public interface IJobQueue
    {
        void Push(Action job);
    }
    public class JobQueue : IJobQueue
    {
        Queue<Action> _jobQueue = new Queue<Action>();
        object _lock = new object();
        bool _flush = false;

        public void Push(Action job)
        {
            // _flush가 true면 다른 스레드가 처리 중이므로 새 flush 시작 없이 바로 탈출하여 lock 병목 최소화
            bool flush = false;

            lock (_lock)
            {
                _jobQueue.Enqueue(job);
                //_flush가 작업중이 아니라면
                if (!_flush)
                    flush = _flush = true;//_flush를 잠군다
            }


            if (flush)
                Flush();
        }

        void Flush()
        {
            while (true)
            {
                Action action = Pop();
                //잡큐의 액션을 뽑아왔는데 액션이 없으면 루프를 중단
                if (action == null)
                {
                    return;
                }
                //정상일 시 진행하고 새로운 루프로
                action.Invoke();
            }
        }

        Action Pop()
        {
            lock ( _lock)
            {
                if (_jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
}
