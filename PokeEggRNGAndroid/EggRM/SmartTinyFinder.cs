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
using Gen7EggRNG.Util;
using Pk3DSRNGTool.RNG;

namespace Gen7EggRNG.EggRM {

    public class TinyWorkProducer : IWorkProducer<int>
    {
        public const int MaxValue = 0x10000;
        int currentState;

        public TinyWorkProducer() { currentState = 0; }
        public TinyWorkProducer(int state) { currentState = state; }

        int IWorkProducer<int>.DrawWork()
        {
            return (currentState < MaxValue ? currentState++ : -1);
        }

        bool IWorkProducer<int>.IsComplete()
        {
            return !(currentState < MaxValue);
        }

        void IWorkProducer<int>.Reset()
        {
            currentState = 0;
        }

        public int GetCurrentState() {
            return currentState;
        }

        public void SetState(int state) {
            currentState = state;
        }
    }

    class SmartTinyFinder
    {
        // Advance based on ShinyCharm
        private int Advance;
        public readonly uint[] NatureList = new uint[8];

        // Actions to be performed by worker threads
        public Action UpdateProgress;
        public Action<string> UpdateSeeds;
        public Action onFinishAction;

        // Seeds
        private object seedLock = new object();

        // Thread management
        TinyWorkProducer twp;
        LocalThreadManager<int> ltManager;

        // States
        //int currentState = 0;
        bool isOngoing = false;


        public SmartTinyFinder() : base()
        {
            twp = new TinyWorkProducer(0);
        }

        public SmartTinyFinder(int currentState)
        {
            twp = new TinyWorkProducer(currentState);
        }

        public int GetProgress() {
            return twp.GetCurrentState();
        }
        public bool IsSearchOngoing() {
            return isOngoing;
        }
        public void Abort()
        {
            ltManager?.Abort();
        }

        public void Pause() {
            if (ltManager != null)
            {
                if (!ltManager.IsPaused())
                {
                    ltManager.Pause();
                    System.Threading.Thread.Sleep(500);
                    //Progress = twp.GetCurrentState();
                }
            }
        }

        public void SetState(int startState) {
            twp.SetState(startState);
        }

        public int GetCurrentState() {
            return twp.GetCurrentState();
        }

        public void Resume() {
            ltManager?.Resume();
        }
        public bool IsPaused() {
            return (ltManager != null ? ltManager.IsPaused() : false);
        }

        public void PreStart(int numThreads)
        {
            ltManager = new LocalThreadManager<int>(numThreads, -1, twp, findseedWorker, onFinishAction);
        }

        public void Execute() {
            if (!isOngoing)
            {
                isOngoing = true;
                ltManager.ExcecuteAsync();
            }
        }
        
        private void parseseed(uint seed)
        {
            var rng = new TinyMT(seed);
            string seedString = rng.CurrentState().ToString();
            lock (seedLock)
            {
                UpdateSeeds(seedString);
            }
        }

        public bool SetFinder(uint[] list, bool HasShinyCharm = false)
        {
            if (list.Length != 8)
                return false;
            Advance = HasShinyCharm ? 12 : 10; // Advancement After IVs
            list.CopyTo(NatureList, 0);
            return true;
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

        private void findseedWorker(int workId) {
            uint minseed, maxseed;
            if (workId < 0x8000)
            {
                minseed = (uint)workId * 0x10000;
                maxseed = minseed + 0xFFFF;
                findseed(minseed, maxseed);
            }
            else {
                minseed = (uint)workId * 0x10000 -1;
                maxseed = minseed + 0x10000;
                findseedrev(maxseed, minseed);

            }
        }

        private void findseed(uint seedmin, uint seedmax)
        {
            for (uint i = seedmin; i <= seedmax; i++)
            {
                if (Check(i))
                    parseseed(i);
            }
            UpdateProgress();
        }

        private void findseedrev(uint seedmax, uint seedmin)
        {
            for (uint i = seedmax; i > seedmin; --i)
            {
                if (Check(i))
                    parseseed(i);
            }
            UpdateProgress();
        }
    }
}