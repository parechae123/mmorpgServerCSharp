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
    //재귀적 락을 허용할지(yes)
    //WriteLock->WriteLock(OK)
    //WriteLock->ReadLock(OK)
    //ReadLock->WriteLock(NO)
    //스핀락 정책 (5000번 => Yield)
    public class LockFree
    {
        // 초기 상태 (락 없음)
        const int EMPTY_FLAG = 0x00000000;

        // 상위 16비트: WriteLock을 획득한 스레드 ID 저장 (0x7FFF0000 = 0111 1111 1111 1111 0000 0000 0000 0000)
        const int WRITE_MASK = 0x7FFF0000;

        // 하위 16비트: ReadLock의 개수 저장 (0x0000FFFF = 0000 0000 0000 0000 1111 1111 1111 1111)
        const int READ_MASK = 0x0000FFFF;

        // 스핀락 최대 시도 횟수 (이 값을 초과하면 Thread.Yield() 호출)
        const int MAX_SPIN_COUNT = 5000;

        // [unused(1 bit)] [WriteThreadId(15 bits)] [ReadCount(16 bits)]
        int _flag = EMPTY_FLAG;

        // 동일 스레드가 여러 번 WriteLock을 획득할 경우를 대비한 카운트
        int _writeCount = 0;

        /// <summary>
        /// WriteLock을 획득하여 쓰기 권한을 얻음
        /// </summary>
        public void WriteLock()
        {
            // 현재 WriteLock을 가지고 있는 스레드 ID 추출
            // _flag & WRITE_MASK: 상위 16비트만 남기고 나머지 16비트를 0으로 설정
            // >> 16: 상위 16비트를 우측으로 이동하여 스레드 ID 값만 추출
            int lockThreadID = (_flag & WRITE_MASK) >> 16;

            // 현재 스레드가 이미 WriteLock을 가지고 있다면 중첩 획득 처리
            if (Thread.CurrentThread.ManagedThreadId == lockThreadID)
            {
                _writeCount++; // 중첩 획득 시 횟수 증가
                return;
            }

            // 현재 스레드 ID를 16비트 왼쪽으로 이동하여 WRITE_MASK와 결합
            // ex) 스레드 ID가 1234라면 (0000 0000 0000 0000 0100 1101 0010 0000)
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;

            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // 현재 _flag 값이 EMPTY_FLAG(= 0)일 경우, 새 값을 설정하여 WriteLock 획득
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1; // 첫 획득이므로 1로 설정
                        return;
                    }
                }

                // 너무 많은 스핀락을 돌 경우 CPU 점유율을 낮추기 위해 양보
                Thread.Yield();
            }
        }

        /// <summary>
        /// WriteLock을 해제하여 다른 스레드가 접근할 수 있도록 함
        /// </summary>
        public void WriteUnlock()
        {
            int lockCount = --_writeCount; // 중첩 획득 시 카운트 감소

            // 마지막으로 락을 해제하는 경우, _flag를 초기 상태로 설정
            if (lockCount == 0)
            {
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
            }
        }

        /// <summary>
        /// ReadLock을 획득하여 데이터를 읽을 수 있도록 함
        /// </summary>
        public void ReadLock()
        {
            // 현재 WriteLock을 가지고 있는 스레드 ID 확인
            int lockThreadID = (_flag & WRITE_MASK) >> 16;

            // 현재 스레드가 WriteLock을 이미 가지고 있다면 ReadLock 추가
            if (Thread.CurrentThread.ManagedThreadId == lockThreadID)
            {
                Interlocked.Increment(ref _flag); // 읽기 카운트 증가
                return;
            }

            // WriteLock이 없는 경우, 스핀락을 통해 ReadLock 획득 시도
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // _flag의 하위 16비트(READ_MASK)를 가져와 현재 읽기 카운트 추출
                    int expected = (_flag & READ_MASK);

                    // 읽기 카운트를 증가시키기 위한 원자적 연산
                    // Interlocked.CompareExchange(ref _flag, expected + 1, expected)
                    // 1) _flag의 값이 expected(현재 읽기 카운트)와 동일하면 expected + 1로 변경
                    // 2) 다르면 아무런 동작 없이 기존 값을 반환 (즉, 실패하면 다시 시도)
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }

                // 너무 많은 스핀락을 돌 경우 CPU 점유율을 낮추기 위해 양보
                Thread.Yield();
            }
        }

        /// <summary>
        /// ReadLock을 해제하여 읽기 카운트를 감소시킴
        /// </summary>
        public void ReadUnLock()
        {
            // 읽기 카운트 감소 (원자적 연산)
            Interlocked.Decrement(ref _flag);
        }
    }
}
