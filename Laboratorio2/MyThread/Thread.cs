using System;
using System.Threading;
using System.Collections.Generic;


namespace MyThread
{

    delegate void ThrWork();

    class ThrPool
    {

        private Queue<ThrWork> request_queue;
        private int bufSize, thrNum;
        private static int mainThreadId = 0;

        public static bool IsMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
        }

        public ThrPool(int thrNum, int bufSize)
        {

            this.bufSize = bufSize;
            this.thrNum = thrNum;
            this.request_queue = new Queue<ThrWork>(bufSize);

        }

        delegate void AssyncInvokeDelegate(ThrWork action);

        public void AssyncInvoke(ThrWork action)
        {

            if (!IsMainThread)
            {

                while (true)
                {
                    action();
                    try
                    {
                        action = this.request_queue.Dequeue();
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                }

                thrNum++;
            }
            else
            {
                mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

                if (thrNum > 0)
                {
                    thrNum--;
                    AssyncInvokeDelegate thread = new AssyncInvokeDelegate(AssyncInvoke);
                    thread.BeginInvoke(action, null, null);
                }
                else
                {
                    if (this.request_queue.Count >= bufSize)
                    {
                        return;
                    }

                    this.request_queue.Enqueue(action);

                }
            }
        }
    }


    class A
    {
        private int _id;

        public A(int id)
        {
            _id = id;
        }

        public void DoWorkA()
        {
            Console.WriteLine("A-{0}", _id);
        }
    }


    class B
    {
        private int _id;

        public B(int id)
        {
            _id = id;
        }

        public void DoWorkB()
        {
            Console.WriteLine("B-{0}", _id);
        }
    }


    public class Threads
    {
        [STAThread]
        public static void Main()
        {
            ThrPool tpool = new ThrPool(5, 10);

            for (int i = 0; i < 5; i++)
            {
                A a = new A(i);
                tpool.AssyncInvoke(new ThrWork(a.DoWorkA));
                B b = new B(i);
                tpool.AssyncInvoke(new ThrWork(b.DoWorkB));
            }
            Console.ReadLine();
        }
    }

}