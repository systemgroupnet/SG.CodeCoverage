using System;
using System.IO;
using System.Threading;

namespace SG.CodeCoverage.Recorder
{
    public static class HitsRepository
    {
        private static int[][] TypeMethodsHits = new int[InjectedConstants.TypesCount][];

        static HitsRepository()
        {
            RecordingController.Server.Initialize();
        }

        /// <summary>
        /// Will be called by static constructor of every instrumented type.
        /// </summary>
        public static void InitType(int typeIndex, int methodsCount)
        {
            lock (TypeMethodsHits)
                TypeMethodsHits[typeIndex] = new int[methodsCount];
        }

        public static void AddHit(int typeIndex, int methodIndex)
        {
            Interlocked.Increment(ref TypeMethodsHits[typeIndex][methodIndex]);
        }

        public static void SaveAndResetHits(string hitsFile)
        {
            var hits = GetAndResetHits();
            HitsFileUtils.SaveHits(hits, hitsFile);
        }

        public static int[][] GetAndResetHits()
        {
            int[][] newHits = new int[InjectedConstants.TypesCount][];
            var hits = TypeMethodsHits;
            lock (TypeMethodsHits)
            {
                for (int i = 0; i < InjectedConstants.TypesCount; i++)
                    newHits[i] = new int[hits[i]?.Length ?? 0];
                TypeMethodsHits = newHits;
            }
            return hits;
        }

        public static void ResetHits()
        {
            lock (TypeMethodsHits)
            {
                for (int i = 0; i < InjectedConstants.TypesCount; i++)
                {
                    var hits = TypeMethodsHits[i];
                    if (hits != null)
                        Array.Clear(hits, 0, hits.Length);
                }
            }
        }

    }
}
