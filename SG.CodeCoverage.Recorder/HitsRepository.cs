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
            SaveHits(hits, hitsFile);
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

        public static void SaveHits(int[][] hits, string hitsFilePath)
        {
            using (var fs = new FileStream(hitsFilePath, FileMode.CreateNew))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(InjectedConstants.InstrumentationUniqueId);
                bw.Write(hits.Length); // Number of total types
                for (int typeIndex = 0; typeIndex < hits.Length; typeIndex++)
                {
                    var typeHits = hits[typeIndex] ?? Array.Empty<int>();
                    ulong sum = 0;
                    bw.Write(typeIndex); // Type index
                    bw.Write(typeHits.Length); // Number of instrumented methods in the type
                    foreach (var methodHit in typeHits)
                    {
                        bw.Write(methodHit); // Hits count for the method
                        sum += (ulong)methodHit;
                    }
                    bw.Write(sum); // Sum of hits count for the type
                }
            }
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

        public static (Guid uniqueId, int[][]) LoadHits(string hitsFilePath)
        {
            using (var fs = new FileStream(hitsFilePath, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                var uniqueIdStr = br.ReadString();
                int typesCount = br.ReadInt32();
                var hits = new int[typesCount][];
                for (int typeIndex = 0; typeIndex < hits.Length; typeIndex++)
                {
                    var storedTypeIndex = br.ReadInt32();
                    if (storedTypeIndex != typeIndex)
                        throw new InvalidDataException($"Error in hits file. Type index mismatch. Stored in hits file: {storedTypeIndex}, Expected: {typeIndex}");
                    var methodsCount = br.ReadInt32();
                    ulong sum = 0;
                    var typeHits = hits[typeIndex] = new int[methodsCount];
                    for (int methodIndex = 0; methodIndex < methodsCount; methodIndex++)
                        sum += (ulong)(typeHits[methodIndex] = br.ReadInt32());
                    var storedSum = br.ReadUInt64();
                    if (storedSum != sum)
                        throw new InvalidDataException($"Error in hits file. Hits sum mismatch. Stored in file: {storedSum}, Expected: {sum}");
                }
                return (Guid.Parse(uniqueIdStr), hits);
            }
        }
    }
}
