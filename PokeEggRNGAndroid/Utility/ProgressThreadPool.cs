using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util.Concurrent;
using Pk3DSRNGTool.RNG;

namespace Gen7EggRNG.Util
{
    /*public class TinyBlockingQueue : LinkedBlockingQueue {

        public override Java.Lang.Object Poll()
        {
            if ()
            return base.Poll();
        }
    }*/


    public class ProgUncaughtExceptionHandler : Java.Lang.Object, Thread.IUncaughtExceptionHandler
    {
        void Thread.IUncaughtExceptionHandler.UncaughtException(Thread t, Throwable e)
        {
            /*Do nothing for now*/
        }
    }

    public class ProgressThreadFactory : Java.Lang.Object, IThreadFactory
    {
        int num = 0;

        Thread IThreadFactory.NewThread(IRunnable r)
        {
            Thread thr = new Thread(r);
            thr.Name = "TinyThread" + (num++).ToString();
            thr.Priority = (int)ThreadPriority.Background;

            thr.UncaughtExceptionHandler = new ProgUncaughtExceptionHandler();

            return thr;
        }
    }

    public class ProgressThreadPool : Java.Util.Concurrent.ThreadPoolExecutor {

        private bool isPaused = false;
        private Java.Util.Concurrent.Locks.ReentrantLock pauseLock;
        private Java.Util.Concurrent.Locks.ICondition paused;

        public ProgressThreadPool(int minCorePool, int maxCorePool, long keepAliveTime, Java.Util.Concurrent.TimeUnit keepAliveUnit, Java.Util.Concurrent.IBlockingQueue threadQueue, IThreadFactory thrFac) :
            base(minCorePool, maxCorePool, keepAliveTime, keepAliveUnit, threadQueue, thrFac )
        {
            pauseLock = new Java.Util.Concurrent.Locks.ReentrantLock();
            paused = pauseLock.NewCondition();
        }

        protected override void BeforeExecute(Thread t, IRunnable r)
        {
            base.BeforeExecute(t, r);
            pauseLock.Lock();
            try
            {
                while (isPaused) t.Wait();
            }
            catch (InterruptedException ie)
            {
                t.Interrupt();
            }
            finally {
                pauseLock.Unlock();
            }
        }

        public void Pause()
        {
            pauseLock.Lock();
            try
            {
                isPaused = true;
            }
            finally
            {
                pauseLock.Unlock();
            }
        }

        public void Resume()
        {
            pauseLock.Lock();
            try
            {
                isPaused = true;
                paused.SignalAll();
            }
            finally
            {
                pauseLock.Unlock();
            }
        }

    }

    public class TinyFinderRunnable : Java.Lang.Object, IRunnable
    {
        public static List<int> NatureList;
        public static int Advance;
        public static List<string> seeds;
        private static object _TinyLock = new object();


        private uint start;
        private uint end;

        public TinyFinderRunnable(uint s, uint e) {
            start = s;
            end = e;
        }

        void IRunnable.Run()
        {
            if (start < end)
            {
                findseed(start, end);
            }
            else {
                findseedrev(start, end);
            }
        }

        private void parseseed(uint seed)
        {
            var rng = new TinyMT(seed);
            lock (_TinyLock)
            {
                seeds.Add(rng.CurrentState().ToString());
                //UpdateTable(null);
            }
        }

        private bool Check(uint seed)
        {
            TinyMT tiny = new TinyMT(seed);
            for (int i = 0; i < 7; i++)
            {
                tiny.Next(); // Gender
                if (tiny.Nextuint() % 25 != NatureList[i])
                    return false;
                GenerateRest(tiny);
            }
            tiny.Next();
            return tiny.Nextuint() % 25 == NatureList[7];
        }

        private void GenerateRest(TinyMT tiny)
        {
            // Ability
            tiny.Next();

            // IVs Random Advancement
            // Hard to read but efficient
            uint tmp;
            uint[] InheritIV = new uint[2];
            InheritIV[0] = tiny.Nextuint() % 6;
            tiny.Next();
            do { tmp = tiny.Nextuint() % 6; }
            while (tmp == InheritIV[0]);
            tiny.Next(); InheritIV[1] = tmp;
            do { tmp = tiny.Nextuint() % 6; }
            while (tmp == InheritIV[0] || tmp == InheritIV[1]);

            // Rest
            for (int i = Advance; i > 0; i--) // IVs * 7 + EC + PID * 0/2 + 2
                tiny.Next();
        }


        private void findseed(uint seedmin, uint seedmax)
        {
            for (uint i = seedmin; i < seedmax; i++)
            {
                if (Check(i))
                    parseseed(i);
            }
            //UpdateProgress(null);
        }

        private void findseedrev(uint seedmax, uint seedmin)
        {
            for (uint i = seedmax; i > seedmin; --i)
            {
                if (Check(i))
                    parseseed(i);
            }
            //UpdateProgress(null);
        }

        private void FindSeed(object param)
        {
            var seedrange = (uint[])param;
            findseed(seedrange[0], seedrange[1]);
        }
    }

    public class ProgressThreadPoolManager
    {

        private ProgressThreadPool ptp;
        private LinkedBlockingQueue queue;

        public ProgressThreadPoolManager() {
        }

        public void StartWork(int state = 0) {
            queue = new LinkedBlockingQueue();
            if (state == 0)
            {
                List<Runnable> r = new List<Runnable>();
                for (uint i = 0; i < 0x080; ++i) {
                    uint min = i * 0xFFFFF;
                    uint max = (i + 1) * 0xFFFFF;
                    
                }
            }
            else {
                //Determine current state and stuff
            }
            ptp = new ProgressThreadPool(Runtime.GetRuntime().AvailableProcessors(), Runtime.GetRuntime().AvailableProcessors() * 2, 60L, TimeUnit.Seconds, queue, new ProgressThreadFactory());


        }
    }
}