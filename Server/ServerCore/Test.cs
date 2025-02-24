using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Test
    {
        static object _lock = new object();
        SpinLock _lock2 = new SpinLock();
        //System.THreading에서 기본 지원하는 객체
        //존버메타로 시도 하다가 안되면 양보메타로 잠깐 갈아탔다옴
        class Reward
        {

        }
        static ReaderWriterLockSlim _lock3 = new ReaderWriterLockSlim();
        static Reward GetRewardByid(int id)
        {
            _lock3.EnterReadLock();

            _lock3.ExitReadLock();
            return null;
        }
        void AddReward(Reward reward)
        {
            _lock3.EnterWriteLock();
            _lock3.ExitWriteLock();
        }
        
    }
    //재귀적 락을 허용할지(No)
    //스핀락 정책 (5000번 => Yield)
    public class LockFree
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;
        //[unused(1)] [WriteThreadId(15)] [ReadCount(16)]
        int _flag;

        public void WriteLock()
        {
            //아무도 WriteLock or ReadLock을 획득하고 있지 않을 때 경합해서 소유권을 얻는다
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        return;
                    }
                }

                Thread.Yield();
            }
        }
        public void WriteUnlock()
        {

        }
        public void ReadLock()
        {

        }
        public void ReadUnLock()
        {

        }


    }
}
