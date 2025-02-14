
using System;
using System.Threading;

namespace ServerCore
{
    class Prgoram
    {
        static bool _stop = false;

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

        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);
            t.Start();

            Thread.Sleep(1000);
            _stop = true;
            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기");
            t.Wait();

            Console.WriteLine("종료 성공");
        }
    }
}