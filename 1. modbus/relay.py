#!py -3.9-32

import time
import struct
import msvcrt
import itertools

import win32com.client
from pymodbus.client.sync import ModbusTcpClient
from pymodbus.utilities import pack_bitstring, unpack_bitstring


actmsg = win32com.client.Dispatch("ActSupportMsg.ActMLSupportMsg")
actutl = win32com.client.Dispatch("ActUtlType.ActMLUtlType")
client = ModbusTcpClient()


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

print("  press any key to stop", end="\r")

progress_chars = itertools.cycle("-\|/")
while not msvcrt.kbhit():
    print(next(progress_chars), end="\r")

    BITS = 256
    WORDS = BITS // 16

    response = client.read_discrete_inputs(0, BITS)
    values = struct.unpack(f"{WORDS}H", pack_bitstring(response.bits))
    res = actutl.WriteDeviceBlock("X1000", len(values), values)
    if res != 0:
        raise act_error("write device block", res)

    values = [0]*WORDS
    res, values = actutl.ReadDeviceBlock("Y1000", len(values), values)
    if res != 0:
        raise act_error("read device block", res)
    outputs = unpack_bitstring(struct.pack(f"{WORDS}H", *values))
    client.write_coils(0, outputs)

    WORDS = 64

    response = client.read_input_registers(0, WORDS)
    values = response.registers
    res = actutl.WriteDeviceBlock("D1000", len(values), values)
    if res != 0:
        raise act_error("write device block", res)

    values = [0]*WORDS
    res, values = actutl.ReadDeviceBlock("D1100", len(values), values)
    if res != 0:
        raise act_error("read device block", res)
    client.write_registers(0, values)

msvcrt.getch()

actutl.Close()
