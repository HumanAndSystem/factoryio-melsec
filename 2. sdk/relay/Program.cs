using System;
using System.Collections;

using ActUtlTypeLib;
using ActSupportMsgLib;
using EngineIO;

namespace acttest
{
    class ActError : Exception
    {
        static ActSupportMsgClass actmsg;

        public ActError(string Action, int ErrorCode) : base(ActError.GetErrorMessage(Action, ErrorCode)) {
        }

        static internal string GetErrorMessage(string Action, int ErrorCode) {
            if (ActError.actmsg is null) {
                ActError.actmsg = new ActSupportMsgClass();
            }
            string msg;
            ActError.actmsg.GetErrorMessage(ErrorCode, out msg);
            return $"{Action}: {msg}";
        }
    }


    class Relay
    {
        const string PROGRESS_CHARS = "-\\|/";

        public Relay() {

        }

        public void Dump() {
            DumpMemories("Input Bit", MemoryMap.Instance.GetBitMemories(MemoryType.Input));
            DumpMemories("Output Bit", MemoryMap.Instance.GetBitMemories(MemoryType.Output));
            DumpMemories("Input Int", MemoryMap.Instance.GetIntMemories(MemoryType.Input));
            DumpMemories("Output Int", MemoryMap.Instance.GetIntMemories(MemoryType.Output));

            void DumpMemories(string title, Memory[] memories) {
                Console.WriteLine($"{title}");
                foreach (var mem in memories) {
                    if (!string.IsNullOrEmpty(mem.Name)) {
                        Console.WriteLine($"    {mem.Address:d3}({mem.Address:x3}): {mem.Name}");
                    }
                }
            }
        }

        public void Run() {
            var actutl = new ActUtlTypeClass();
            actutl.ActLogicalStationNumber = 2;

            var res = actutl.Open();
            if (res != 0) throw new ActError("open", res);

            Console.Write("  press any key to stop\r");

            int progress_i = 0;
            while (!Console.KeyAvailable)
            {
                Console.Write("{0}\r", PROGRESS_CHARS[progress_i++]);
                if (progress_i >= PROGRESS_CHARS.Length) progress_i = 0;

                {
                    const int BITS = 512;
                    const int WORDS = BITS / 16;

                    var memories = MemoryMap.Instance.GetBitMemories(MemoryType.Input);
                    var bits = new BitArray(BITS);
                    for (int i = 0; i < BITS; i++) {
                        var bit = memories[i];
                        bits.Set(i, bit.Value);
                    }
                    var bytes = new byte[WORDS*2];
                    bits.CopyTo(bytes, 0);
                    var values = new short[WORDS];
                    Buffer.BlockCopy(bytes, 0, values, 0, bytes.Length);
                    res = actutl.WriteDeviceBlock2("X1000", values.Length, ref values[0]);
                    if (res != 0) throw new ActError("write device block", res);

                    res = actutl.ReadDeviceBlock2("Y1000", values.Length, out values[0]);
                    if (res != 0) throw new ActError("read device block", res);
                    Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
                    memories = MemoryMap.Instance.GetBitMemories(MemoryType.Output);
                    bits = new BitArray(bytes);
                    for (int i = 0; i < BITS; i++) {
                        var bit = memories[i];
                        bit.Value = bits.Get(i);
                    }
                }

                {
                    const int WORDS = 256;

                    var values = new int[WORDS];
                    var memories = MemoryMap.Instance.GetIntMemories(MemoryType.Input);
                    for (int i = 0; i < WORDS; i++) {
                        var word = memories[i];
                        values[i] = word.Value;
                    }
                    res = actutl.WriteDeviceBlock("D1000", values.Length, ref values[0]);
                    if (res != 0) throw new ActError("write device block", res);

                    res = actutl.ReadDeviceBlock("D2000", values.Length, out values[0]);
                    if (res != 0) throw new ActError("read device block", res);
                    memories = MemoryMap.Instance.GetIntMemories(MemoryType.Output);
                    for (int i = 0; i < WORDS; i++) {
                        var word = memories[i];
                        word.Value = values[i];
                    }
                }

                MemoryMap.Instance.Update();
            }

            Console.ReadKey();

            res = actutl.Close();
            if (res != 0) throw new ActError("close", res);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var relay = new Relay();
            relay.Dump();
            relay.Run();

            MemoryMap.Instance.Dispose();
        }
    }
}
