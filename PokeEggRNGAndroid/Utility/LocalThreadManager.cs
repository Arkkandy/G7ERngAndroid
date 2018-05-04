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

using System.Threading;

namespace Gen7EggRNG.Util
{

    public interface IWorkProducer<T> {
        T DrawWork();
        bool IsComplete();
        void Reset();
    };
   
    public class LocalThreadManager<T>
    {
        private int maxThreads;
        private int activeCount;
        private List<Thread> pool = new List<Thread>();
        private object workFetchLock = new object();

        private IWorkProducer<T> producer;
        private T noWork;
        private Action<T> workTask;
        private Action onFinish;

        private bool isOngoing = false;
        private bool isPaused = false;
        private System.Threading.EventWaitHandle waitCondition = new EventWaitHandle(false, EventResetMode.ManualReset);

        public LocalThreadManager(int numThreads, T noWork, IWorkProducer<T> producer, Action<T> workTask, Action onFinish ) {
            this.maxThreads = numThreads;
            this.producer = producer;
            this.noWork = noWork;
            this.workTask = workTask;
            this.onFinish = onFinish;
        }

        public void ExcecuteAsync() {
            bool canStart = false;
            lock (workFetchLock)
            {
                if (!isOngoing)
                {
                    isOngoing = true;
                    canStart = true;
                }
            }
            if (!canStart) { return; }

            System.Threading.Thread workerThr = new System.Threading.Thread(
                () =>
                {
                    isPaused = false;
                    activeCount = 0;
                    for (int i = 0; i < maxThreads; ++i)
                    {
                        AddWorker();
                    }
                    for (int i = 1; i <= maxThreads; ++i)
                    {
                        pool[i].Join();
                    }
                    if (producer.IsComplete()) {
                        onFinish();
                    }
                    pool.Clear();
                    isOngoing = false;
                })
            {
                IsBackground = true,
                Name = "MainBackWorker"
            };

            pool.Add(workerThr);
            workerThr.Start();
        }

        private void AddWorker() {
                System.Threading.Thread workerThr = new System.Threading.Thread(WorkerFunction)
                {
                    IsBackground = true,
                    //Priority = System.Threading.ThreadPriority.Normal,
                    Name = "BackWorker" + pool.Count
                };
                pool.Add(workerThr);
                workerThr.Start();
                activeCount++;
        }

        private void WorkerFunction() {
            while (true) {
                while (isPaused) {
                    waitCondition.WaitOne();
                }
                
                T workLoad;
                lock (workFetchLock) {
                    workLoad = producer.DrawWork();
                }
                if (workLoad.Equals( noWork )) { break; }
                else { workTask(workLoad); }
            }
        }

        public void Abort() {
            lock (workFetchLock)
            {
                if (isOngoing)
                {
                    for (int i = 1; i < pool.Count; ++i)
                    {
                        pool[i].Abort();
                    }
                    pool[0].Abort();

                    pool.Clear();

                    isOngoing = false;
                }
            }
        }

        public void Pause() {
            isPaused = true;
        }

        public void Resume(){
            isPaused = false;
            waitCondition.Set();
        }

        public bool IsPaused() {
            return isPaused;
        }
        /*private void Complete() {

        }*/
    }
}