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
            res = actctrl.Close();
            if (res != 0) throw new ActError(res);
        }
    }
}
