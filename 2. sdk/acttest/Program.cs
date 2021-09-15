using System;

using ActUtlTypeLib;
using ActSupportMsgLib;

namespace acttest
{
    class ActError : Exception
    {
        static ActSupportMsgClass actmsg;

        public ActError(int ErrorCode) : base(ActError.GetErrorMessage(ErrorCode)) {
        }

        static internal string GetErrorMessage(int ErrorCode) {
            if (ActError.actmsg is null) {
                ActError.actmsg = new ActSupportMsgClass();
            }
            string msg;
            ActError.actmsg.GetErrorMessage(ErrorCode, out msg);
            return msg;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var actctrl = new ActUtlTypeClass();
            actctrl.ActLogicalStationNumber = 2;

            var res = actctrl.Open();
            if (res != 0) throw new ActError(res);

            int value;
            res = actctrl.GetDevice("Y1000", out value);
            Console.WriteLine("{0}", value);
            value = (value == 0 ? 1 : 0);
            actctrl.SetDevice("Y1000", value);
            res = actctrl.GetDevice("Y1000", out value);
            Console.WriteLine("{0}", value);

            res = actctrl.Close();
            if (res != 0) throw new ActError(res);
        }
    }
}
