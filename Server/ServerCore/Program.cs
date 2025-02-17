
using Microsoft.VisualBasic;
using System;
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

        /*        static void Main(string[] args)
                {
                    멀티쓰레드 예제
                    Task t = new Task(ThreadMain);
                    t.Start();

                    Thread.Sleep(1000);
                    _stop = true;
                    Console.WriteLine("Stop 호출");
                    Console.WriteLine("종료 대기");
                    t.Wait();

                    Console.WriteLine("종료 성공");
                }*/

        /*        static void Main(string[] args)
                {
                    공간 지연성 예제, 인접한 메모리를 캐시해옴
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

        //메모리 배리어 예제
        static volatile int x = 0;
        static volatile int y = 0;
        static volatile int r1 = 0;
        static volatile int r2 = 0;

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

        }
    }
}