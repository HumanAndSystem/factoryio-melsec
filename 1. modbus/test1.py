import time
from pymodbus.client.sync import ModbusTcpClient


client = ModbusTcpClient()
client.write_coil(0, 1)
time.sleep(2)
client.write_coil(0, 0)
