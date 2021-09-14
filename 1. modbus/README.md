이 방안은 Factory IO를 MODBUS Server로 설정하고, 
MX-Component로 PLC 측의 X/Y/D 디바이스를 MODBUS Input과 Coil 그리고 Register로 중계하는 것이다.
프로그램은 Python으로 할 것이다. 

# 준비

## Factory IO 준비 

우선 Factory IO에 검토에 사용할 'Automated Warehouse' [Scene](https://docs.factoryio.com/manual/scenes/)을 올린다. 

열린 Scene에는 아직 'Drive'가 설정되지 않은 상태다. 
단축키 F4를 누루거나
메뉴에서 File-Drives를 선택하거나
혹은 화면의 오른쪽 아래에 칩 모양의 아이콘에 'None'이라고 쓰여 있는 것이 현재 설정된 Drive인데 
여기를 마우스로 눌러도 Drive를 선택할 수 있는 [창](https://docs.factoryio.com/manual/drivers/)으로 바뀐다. 

상단의 DRIVER 선택을 ['Modbus TCP/IP Server'](https://docs.factoryio.com/manual/drivers/modbus-server/)로 바꾼다. 

대부분은 기본으로 설정된 것을 그대로 사용할 것이다. 
오른쪽 상단에 CONFIGURATION를 선택하면 Server에 대한 좀 더 세부적인 설정이 나타나는데 
Slave ID가 0인 것을 확인한다. 아니면 0으로 바꾼다. 
프로그램에 pymodbus라는 라이브러리를 사용할 텐데
이 라이브러리의 Slave ID 기본값이 0이라 번거롭지 않도록 하기 위해서다.
그리고 Network adapter를 Software Loopback Interface로 바꿔서 
Host를 127.0.0.1로 할 것이다.
마찬가지로 pymodbus의 기본값에 맞춰서 번거롭지 않도록 하기 위해서다.

## GX Works2와 MX-Componet4 준비

GX Works2로는 아무 QCPU로 프로젝트 하나를 새로 만들고,
MX-Componet4의 `Communication Setup Utility`로는
'GX Simulator2'을 대상으로 하는 `Logical station number`를 하나 만든다.
Python 프로그램은 이게 2번이라고 가정할 것이다.

![gx_simulator2](gx_simulator2.png)

## Python과 pymodbus 준비

[Python](https://www.python.org/)은 [3.9.7](https://www.python.org/downloads/release/python-397/) 버전을 사용할 것이고,
[pymodbus](https://pypi.org/project/pymodbus/)와 [pywin32](https://pypi.org/project/pywin32/)를 추가해 준다.
`pywin32`는 COM 라이브러리인 MX-Component를 사용하는데 필요하다.

64비트 프로그램은 MX-Component5부터 지원하므로
검토에 사용할 MX-Component4는 32비트 프로그램으로 해야 한다.
따라서 Python도 32비트로 준비해야 한다.

# 시범

## Modbus

PLC의 디바이스를 중계하기 전에 Modbus만으로 동작을 확인해 볼 것이다.

Factory IO가 Server이므로 Python은 Client가 되어야 하므로
pymodbus의 ModbusTcpClient를 가져온다.

```python
from pymodbus.client.sync import ModbusTcpClient


client = ModbusTcpClient()  # 127.0.0.1:502가 기본값이다.
```

다음에 Coil 하나를 살린 것인데,

```python
client.write_coil(0, 1)
```

![tag_map](tag_map.png)

Tag 연결 지도를 보면 'Coil 0'이 'Entry Conveyor'다.
따라서 'Coil 0'에 1을 쓰면 'Entry Conveyor'가 동작할 것이다.

`test1.py`는 'Coil 0'을 On 시켰다가 2초 후에 Off 시킨다.

```python
import time
from pymodbus.client.sync import ModbusTcpClient


client = ModbusTcpClient()
client.write_coil(0, 1)
time.sleep(2)
client.write_coil(0, 0)
```

Factory IO에서 Scene을 실행시키고, `test1.py`을 실행시키면
'Entry Conveyor'가 동작해서 Pallet가 움직이는 것을 확인할 수 있다.


## MX-Component

`Logical station number`를 사용할 것이므로 사용할 MX-Component Control은 `ActUtlType`이다.

```python
actutl = win32com.client.Dispatch("ActUtlType.ActMLUtlType")
```

만들어진 COM 객체에 `ActLogicalStationNumber`를 설정하고 `Open`을 호출한다.

```python
actutl.ActLogicalStationNumber = 2
actutl.Open()
```

`Logical station number` 2번은 `Communication Setup Utility`로 미리 등록해 둬야 한다.

```python
import win32com.client


actmsg = win32com.client.Dispatch("ActSupportMsg.ActMLSupportMsg")
actutl = win32com.client.Dispatch("ActUtlType.ActMLUtlType")


def act_error(acrion, res):
    res, msg = actmsg.GetErrorMessage(res)
    if res != 0:
        return Exception(res)
    logger.error(f"error {acrion}: {msg}")
    return Exception(msg)


station = 2
actutl.ActLogicalStationNumber = station
res = actutl.Open()
if res != 0:
    raise act_error("open", res)

actutl.Close()
```

`test2.py`는 단순히 2번을 열었다가 닫는 과정인데,
실행해 보면 에러가 생긴다.
`GX Simulator2`가 실행되어 있지 않기 때문이다.

```
Traceback (most recent call last):
  File "D:\jobs\factoryio-melsec\1. modbus\test2.py", line 21, in <module>
    raise act_error("open", res)
Exception: GX Simulator2 unstart error
GX Simulator2 did not start.
The corrective action is as follows:
 Start GX Simulator2.

<ErrorCode:1809001[Hex]>
```

GX Works2로 적당한 프로젝트 하나를 만들고,
Debug-Start/Stop Simulation  메뉴로 Simulator를 시작한 후에
다시 `test2.py`를 해 보면 에러 없이 바로 종료하는 것을 확인할 수 있다.

읽기, 쓰기도 확인하기 위해 GetDevice와 SetDevice를 사용한다.

```python
res, y = actutl.GetDevice("Y1000")
print(y)
res = actutl.SetDevice("Y1000", 1)
res, y = actutl.GetDevice("Y1000")
print(y)
```

위 내용을 포함한 `test3.py`를 실행해 보면
0인 Y1000의 값이 1로 되는 것을 확인할 수 있다.
GX Works2의 모니터링을 통해서도 값이 바뀌는 것을 확인할 수 있다.

# 중계

## 중계 범위

DRIVE의 CONFIGURATION에서 I/O Points의 count 값을 바꿔보면
Bit인 Input와 Coil은 각각 256개, Word인 Input/Output Register는 각각 64개까지 사용할 수 있다.
그래서 X1000부터 X10FF를 Input으로 Y1000부터 Y10FF까지를 Coil로 중계하고,
D1000부터 D1063까지는 Input Register로 D1100부터 D1163까지는 Output Register로 중계할 것이다.

## 비트 중계

센서등의 입력값을 Modbus를 통해 Factory IO에서 읽는다.

```python
response = client.read_discrete_inputs(0, BITS)
```

`BITS`는 256이고, `response.bits`에 256개의 값이 리스트로 들어 있다.
비트 각각을 `SetDevice`로 PLC 쪽에 전달해도 되지만,
워드 형태로 바꿔서 `WriteDeviceBlock`을 사용하는 것이 빠르다.

```python
values = struct.unpack(f"{WORDS}H", pack_bitstring(response.bits))
res = actutl.WriteDeviceBlock("X1000", len(values), values)
```

256개의 비트는 16개의 워드로 압축되고, X1000부터 기록된다.

컨베이어등을 동작시키는 출력인 Coil은 반대의 과정을 거친다.
우선 PLC의 Y1000부터 16워드를 읽는다.

```python
values = [0]*WORDS
res, values = actutl.ReadDeviceBlock("Y1000", len(values), values)
```

`values`에는 16개의 워드값이 리스트 형태로 들어 있다.

```python
outputs = unpack_bitstring(struct.pack(f"{WORDS}H", *values))
client.write_coils(0, outputs)
```

워드값을 비트로 바꾼다. 결과적으로 `outputs`는 256개의 비트값이다.
이번 에는 이 값들이 Modbus를 통해 Factory IO 쪽으로 전달된다.

##  워드 중계

D1000으로 전달되는 값은 Factory IO에서 만들어지는 값으로
PLC 입장에서는 입력값이다.

```python
response = client.read_input_registers(0, WORDS)
values = response.registers
res = actutl.WriteDeviceBlock("D1000", len(values), values)
```

비트 처리 과정에서의 WORDS는 16이었지만, 여기서는 64다.

```python
    values = [0]*WORDS
    res, values = actutl.ReadDeviceBlock("D1100", len(values), values)
    client.write_registers(0, values)
```

D1100의 값은 PLC에서 Factory IO 쪽으로 보내는 값이다.

검토에 사용된 `Automated Warehouse`는 제어하는 방식에 따라
`Numerical`, `Analog`, `Digital`, `Digital & Analog`의 4가지 설정이 가능한데
기본값은 `Numerical` 이다.
이 경우에는 PLC가 Factory IO로 전달하는 값만 있고,
PLC 쪽으로 전달되는 워드값은 없다.

따라서 D1000에 값이 들어오는 것을 확인하려면
제어 방식을 바꿔야 한다.

## 전체 중계 프로그램

```python
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
```

전체 프로그램은 `relay.py`에 작성되어 있다.
