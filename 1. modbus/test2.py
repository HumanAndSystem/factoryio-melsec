#!py -3.9-32

import win32com.client


actmsg = win32com.client.Dispatch("ActSupportMsg.ActMLSupportMsg")
actutl = win32com.client.Dispatch("ActUtlType.ActMLUtlType")


def act_error(acrion, res):
    res, msg = actmsg.GetErrorMessage(res)
    if res != 0:
        return Exception(res)
    return Exception(msg)


station = 2
actutl.ActLogicalStationNumber = station
res = actutl.Open()
if res != 0:
    raise act_error("open", res)

actutl.Close()
