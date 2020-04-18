using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SG.CodeCoverage.Recorder
{
    public static class HitsRepository
    {
        private static int[][] TypeMethodsHits = new int[InjectedConstants.TypesCount][];

        static HitsRepository()
        {
            RecordingControllerServer.Initialize();
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
            SaveHits(hits, hitsFile);
        }

        private static int[][] GetAndResetHits()
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

        private static void SaveHits(int[][] hits, string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(hits.Length);
                foreach (var typeHits in hits)
                {
                    var length = typeHits?.Length ?? 0;
                    if (length > 0)
                    {
                        foreach (var methodHit in typeHits)
                            bw.Write(methodHit);
                    }
                }
            }
        }
    }
}
