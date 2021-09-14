이 방안은 Factory IO를 MODBUS Server로 설정하고, 
MX-Component로 PLC 측의 X/Y/D 디바이스를 MODBUS Input과 Coil 그리고 Register로 중계할 것이다. 
프로그램은 Python으로 할 것이다. 

## Factory IO 준비 

우선 Factory IO에 검토에 사용할 'Automated Warehouse' Scene을 올린다. 

열린 Scene에는 아직 'Drive'가 설정되지 않은 상태다. 
단축키 F4 
메뉴 File-Drives 
혹은 화면의 오른쪽 아래에 칩 모양의 아이콘에 'None'이라고 쓰여 있는 것이 현재 설정된 Drive인데 
여기를 마우스로 눌러도 Drive를 선택할 수 있는 창으로 바뀐다.

