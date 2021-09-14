[Factory IO](https://factoryio.com/)를 만져보게 된 것은 '3D Factory Simulation'라는 문구에 꽂혀서 이다. 
제작 중인 설비를 시뮬레이션하면 설비없이 미리 하는 개발에 도움을 받을 수 있지 않을까 해서였다. 
하지만 결론적으로 그렇게 사용할 수는 없었다. 

홈페이지에 '3D Factory Simulation'보다 더 굵고 크게 쓰여 있는 문구는 'Next-Gen PLC Training'이다. 
분명하게 PLC를 훈련하기 위한 용도라고 밝히고 있다.

애초 프로그램의 용도가 이렇다 보니 
시뮬레이션할 설비를 구성하기 위한 컨베이어나 각종 센서등 
내부적으로 'Part'라고 부르는 것을 사용자가 새로 만들 수 없다. 
제공되는 것만으로 '훈련용' 설비를 만들 수 있을 뿐이다. 
그러니 개발해야 할 실제 설비를 시뮬레이션해서 개발에 도움을 받는 것은 안 되는 일이었던 것이다.

처음 목적은 이룰 수 없더라도
개발 설비에 적용하려면
Factory IO가 제공하지 않는 MELSEC PLC을 연결할 방안이 필요하다.
이는 검토해 봤다.

가능한 방안은 두 가지다.
첫째는 범용이라고 할 수 있는 MODBUS를 매개로 사용하는 것이고,
두 번째는 [SDK](https://docs.factoryio.com/sdk/)를 사용하는 것이다.

어디까지나 훈련이니까 
PLC도 실물이 아니라 GX Works의 Simulation 기능을 사용할 것이다. 
그러려면 MX-Component도 필요하다. 

Factory IO는 각종 Part로 만들어진 훈련용 설비를 ['Scene'](https://docs.factoryio.com/manual/scenes/)이라고 표현하고, 
미리 만들어진 십여 가지를 제공한다. 
검토에는 이 중에서 ['Automated Warehouse'](https://docs.factoryio.com/manual/scenes/automated-warehouse/index.html)를 사용할 것이다.
