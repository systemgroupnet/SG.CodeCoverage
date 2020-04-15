using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SG.CodeCoverage.Recorder
{
    public class HitsRepository
    {
        private static int[][] ClassMethodsHits = new int[InjectedConstants.ClassCount][];

        static HitsRepository()
        {
            RecordingControllerServer.Initialize();
        }

        /// <summary>
        /// Will be called by static constructor of every instrumented class.
        /// </summary>
        public static void InitClass(int classIndex, int methodsCount)
        {
            lock(ClassMethodsHits)
                ClassMethodsHits[classIndex] = new int[methodsCount];
        }

        public static void AddHit(int classIndex, int methodIndex)
        {
            Interlocked.Increment(ref ClassMethodsHits[classIndex][methodIndex]);
        }

        public static void SaveAndResetHits(string hitsFile)
        {
            var hits = GetAndResetHits();
            SaveHits(hits, hitsFile);
        }

        private static int[][] GetAndResetHits()
        {
            int[][] newHits = new int[InjectedConstants.ClassCount][];
            var hits = ClassMethodsHits;
            lock(ClassMethodsHits)
            {
                for (int i = 0; i < InjectedConstants.ClassCount; i++)
                    newHits[i] = new int[hits[i].Length];
                ClassMethodsHits = newHits;
            }
            return hits;
        }

        private static void SaveHits(int[][] hits, string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(hits.Length);
                foreach(var classHits in hits)
                {
                    bw.Write(classHits.Length);
                    foreach (var methodHit in classHits)
                        bw.Write(methodHit);
                }
            }
        }
    }
}
