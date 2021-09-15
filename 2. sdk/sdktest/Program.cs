using System;

using EngineIO;

namespace acttest
{
    class Program
    {
        static void Main(string[] args)
        {
            var conveyor = MemoryMap.Instance.GetBit(0, MemoryType.Output);
            MemoryMap.Instance.Update();
            conveyor.Value = !conveyor.Value;
            MemoryMap.Instance.Update();

            MemoryMap.Instance.Dispose();
        }
    }
}
