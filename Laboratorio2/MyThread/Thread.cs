using System;
using System.Threading;
using System.Collections.Generic;


namespace MyThread
{

    delegate void ThrWork();

    class ThrPool
    {

        private Thread[] thread_list;

        private ThrWork[] request_queue;
        private int write_index = 0, read_index = 0, bufSize = 0;

        public ThrPool(int thrNum, int bufSize)
        {

            this.bufSize = bufSize;
            this.thread_list = new Thread[thrNum];
            this.request_queue = new ThrWork[bufSize];
            for (int i = 0; i < thrNum; i++)
            {
                ThreadStart tw = new ThreadStart(Worker);
                this.thread_list[i] = new Thread(tw);
                this.thread_list[i].Start();
            }

        }

        public void Worker()
        {

            ThrWork action;

            while (true)
            {
                int new_index = (this.read_index + 1) % this.bufSize;

                Monitor.Enter(this.request_queue);
                if (this.request_queue[new_index] == null)
                {
                    Monitor.Exit(this.request_queue);
                    continue;
                }
                this.read_index = new_index;
                action = this.request_queue[new_index];
                this.request_queue[new_index] = null;
                Monitor.Exit(this.request_queue);
                action();
            }

        }

        public void AssyncInvoke(ThrWork action)
        {

            int new_index = (this.write_index + 1) % this.bufSize;

            Monitor.Enter(this.request_queue);
            Thread.Sleep(10);
            if (this.request_queue[new_index] != null)
            {
                Monitor.Exit(this.request_queue);
                return;
            }
            this.write_index = new_index;
            this.request_queue[new_index] = action;
            Monitor.Exit(this.request_queue);

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
        public void Original()
        {
            ThrPool tpool = new ThrPool(5, 10);
            //ThrWork work = null;
            for (int i = 0; i < 50; i++)
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