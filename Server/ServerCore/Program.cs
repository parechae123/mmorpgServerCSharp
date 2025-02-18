
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Threading;

namespace ServerCore
{

    
    class Prgoram
    {
        volatile static bool _stop = false;

        static void ThreadMain()
        {
            Console.WriteLine("쓰래드 시작");

            while (_stop == false)
            {

            }
            Console.WriteLine("쓰래드 종료");
        }

        static void MainThread()
        {
            Console.WriteLine("Hello Thread!");
        }

        /*        멀티쓰레드 예제
         *        static void Main(string[] args)
                {
                    
                    Task t = new Task(ThreadMain);
                    t.Start();

                    Thread.Sleep(1000);
                    _stop = true;
                    Console.WriteLine("Stop 호출");
                    Console.WriteLine("종료 대기");
                    t.Wait();

                    Console.WriteLine("종료 성공");
                }*/

        /*        공간 지연성 예제, 인접한 메모리를 캐시해옴
         *        static void Main(string[] args)
                {
                    
                    int[,] arr = new int[10000,10000];

                    {
                        long now = DateAndTime.Now.Ticks;
                        for (int y = 0;y<10000; y++)
                        {
                            for (int x = 0; x < 5000; x++)
                            {
                                arr[y, x] = 1;
                            }
                        }
                        long end = DateAndTime.Now.Ticks;
                        Console.WriteLine($"(x,y) 순서 걸린 시간{end-now}");
                    }
                    {
                        long now = DateAndTime.Now.Ticks;
                        for (int y = 0; y < 10000; y++)
                        {
                            for (int x = 0; x < 10000; x++)
                            {
                                x++;
                                arr[y, x] = 1;
                            }
                        }
                        long end = DateAndTime.Now.Ticks;
                        Console.WriteLine($"(y,x) 순서 걸린 시간{end - now}");
                    }

                }*/
        /*      메모리 배리어 예제

                static int x = 0;
                static int y = 0;
                static int r1 = 0;
                static int r2 = 0;

                //1)Full memory barrier (어셈블리 명령어 MFENCE, C# Thread.MomoryBarrior) : Store,Load 둘 다 막는다
                //2)Store Memory Barrier (어셈블리 명령어 SFENCE) : Store만 막는다
                //3)Load Memory Barrier (어셈블리 명령어 LFENCE) : Load만 막는다
                static void Thread_1()
                {
                    y = 1;  //Store y
                    //컴파일러가 이 코드를 재배치 할 수 있기에 무한루프여야 할 코드가 종류되는 것
                    Thread.MemoryBarrier();// 이전에 있는 코드는 재배치 하지마라
                    r1 = x; //Load x
                }

                static void Thread_2()
                {
                    x = 1;  //Store x
                    //컴파일러가 이 코드를 재배치 할 수 있기에 무한루프여야 할 코드가 종류되는 것
                    Thread.MemoryBarrier();// 이전에 있는 코드는 재배치 하지마라
                    r2 = y; //Load y
                }

                static void Main(string[] args)
                {
                    int count = 0;
                    while (true)
                    {
                        count++;
                        x = y = r1 = r2 = 0;

                        Task t1 = new Task(Thread_1);
                        Task t2 = new Task(Thread_2);
                        t1.Start();
                        t2.Start();

                        Task.WaitAll(t1, t2);

                        if (r1==0&&r2 == 0)
                        {
                            break;
                        }

                    }
                    Console.WriteLine($"{count}번 만에 빠져나옴");

                }*/

        /*        //메모리 베리어 예제2
                //Store와 Load가 한 세트로 있지 않을 경우
                int _answer;
                bool _complete;

                void A()
                {
                    _answer = 123;
                    Thread.MemoryBarrier();     //baarirer1
                    _complete = true;          //메모리베리어는 Store와 Load를 한 세트 식 막는데 이런 경우엔 Store만 두번이기에 다음과같이 배치
                    Thread.MemoryBarrier();     //baarirer2
                }

                void B()
                {
                    Thread.MemoryBarrier();     //baarirer3
                    if (_complete)
                    {
                        Thread.MemoryBarrier();     ////baarirer4
                        Console.WriteLine(_answer);
                        //Load만 두번이기에 다음과같이 배치
                    }
                }
                public static void Main(string[] args)
                {
                    Prgoram program = new Prgoram();
                    program.A();
                    program.B();
                }   */

        #region 공유변수 예제
        /*        //공유변수 예제(공유변수의 원자성)

                static int number = 0;
                static object _obj = new object();
                static void Thread_1()
                {
                    for (int i = 0; i < 100000; i++)
                    {
                        //상호배제 Mutual Exclusive

                        lock (_obj)
                        {
                            number++;
                        }

                        *//*                Monitor.Enter(_obj);//문 잠금
                                        {
                                            number++;
                                            //enter와 exit 구문은 Lock문에 내장되어 있음, 사용 예제를 표현

                                        }
                                        Monitor.Exit(_obj);//문 잠금 해제*/


        /* interlocked를 활용한 양자성 해결예제, intOnly, //잘못된 예제 == number++;
        //잘못된 예제인 이유 == Thread1과 2가 각각 동시에 돌기에
        //다음 number의 수는 Thread1의 output일지 2의 output일지 알 수 없어 발생하는 오류
        Interlocked.Increment(ref number);*//*
    }
}

//데드락 DeadLock : Monitor.Exit으로 풀어주지 않았을 시

static void Thread_2()
{
    for (int i = 0; i < 100000; i++)
    {


        lock (_obj)
        {
            number--;
        }

        *//*                Monitor.Enter(_obj);
                        number--;
                        Monitor.Exit(_obj);*/
        /*//잘못된 예제 == number--;
        Interlocked.Decrement(ref number);*//*


    }
}
static void Main(string[] args)
{
    Task t1 = new Task(Thread_1);
    Task t2 = new Task(Thread_2);
    t1.Start();
    t2.Start();

    Task.WaitAll(t1, t2);

    Console.WriteLine(number);
}*/
        #endregion

        #region 데드락 케이스
        /*static int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {
            for (int i = 0; i < 100; i++)
            {
                SessionManager.Test();
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 100; i++)
            {
                UserManager.test();
            }
        }
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            Thread.Sleep(100);
            t2.Start();
            Task.WaitAll(t1, t2);
            
            Console.WriteLine(number);
        }
*/
        #endregion
        #region 스핀락 케이스
        static int _num = 0;
        static spinLock _lock = new spinLock();

        static void Thread_1()
        {
            for (int i = 0; i < 10000000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.release();
            }
        }   
        static void Thread_2()
        {
            for (int i = 0; i < 10000000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.release();
            }
        }   
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();
            Task.WaitAll(t1, t2);
            Console.WriteLine($"테스크 완료 최종 값 : {_num}");
        }

        #endregion
    }
    #region 데드락 테스트 Define
    class SessionManager
    {
        static FastLock _lock = new FastLock(2);
        public static void TestSession()
        {
            lock (_lock)
            {

            }
        }
        public static void Test()
        {
            lock (_lock)
            {
                UserManager.TestUser();
            }
        }

    }
    class UserManager
    {
        static FastLock _lock = new FastLock(1);

        public static void test()
        {
            lock (_lock)
            {
                SessionManager.TestSession();
            }
        }
        public static void TestUser()
        {
            lock (_lock)
            {

            }
        }
    } 
    class FastLock
    {
        //아이디를 대조하여 데드락을 방지하는 꼼수,
        //근데 실제 사용은 하지 말자
        //데드락을 방지하는 방법은 데드락을 일으키는 상황을 만들지 않는 것!!!
       
        public int id;
        public FastLock(int id)
        {
            this.id = id;
        }
    }
    #endregion
    #region 락 대기 구현
    class spinLock
    {
        volatile int _locked = 0;
        public void Acquire()
        {
            while (true)
            {
                //함수 자체는 해당 포인터의 값을 2번째 매개변수로 바꿔주지만
                //리턴하는 값 자체는 기존의 값을 리턴한다
                //해당 값이 필요한 이유 = 멀티쓰레드 환경에서 해당값을 가져오면 양자성으로 인해 원할안 spinLock이 구현되지 않음
                /*                int original = Interlocked.Exchange(ref _locked, 1);
                                if (original == 0)
                                {
                                    break;
                                }*/

                int expected = 0;
                int desired = 1;
                //origin value가 expected와 같을 시(타 쓰래드에서 lock되어있지 않을 시)
                if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected) break;
                
                Thread.Sleep(1);    //무조건 휴식 => 1ms정도 쉬고 싶다,OS 스케튤러에 따라 조금식 달라지지만 1ms를 목표로 함
                //Thread.Sleep(0);    //조건부 양보 => 나보다 우선순위가 낮은 애들한테는 양보 불가 => 우선순위가 나보다 같거나 높은 쓰레드가 없으면 다시 본인에게
                //Thread.Yield();     //관대한 양보 => 관대하게 양보할테니, 지금 실행이 가능한 쓰레드가 있으면 실행하쇼 => 실행 가능 한 애가 없으면 남은 시간 소진
            }


/*잘못된 구현에제
            while (_locked)
            {
                //잠김이 풀리기를 기다린다
            }

            //내꼬얌
            _locked = true;*/
        }   
        public void release()
        {
            //Interlocked.Exchange(ref _locked, 0);
            _locked = 0;
        }   
    }
    #endregion
}