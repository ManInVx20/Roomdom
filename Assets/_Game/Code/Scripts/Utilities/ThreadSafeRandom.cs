#nullable enable
using System;

namespace VinhLB.Utilities
{
    internal static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random? _local;
        private static readonly Random _global = new();

        public static Random Instance
        {
            get
            {
                if (_local is null)
                {
                    int seed;
                    lock (_global)
                    {
                        seed = _global.Next();
                    }

                    _local = new Random(seed);
                }

                return _local;
            }
        }
    }
}