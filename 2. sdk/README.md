[Factory IO SDK](https://github.com/realgamessoftware/factoryio-sdk)는 `EngineIO`라는 이름의 .NET 라이브러리를 제공한다.
이를 통해 Tag를 직접 읽고 쓸 수 있도록 해 준다.

다른 DRIVE가 선택되어 있더라도
SDK를 통한 Tag 읽고 쓰기는 가능하다고 설명서에 분명하게 쓰고 있고, 가능은 하다.
하지만 실제로 해 보면 근방 이건 안 되는 것이라는 것을 느낄 수 있다.

가령은 SDK로 특정 Coil의 값을 ON 시켜도
이게 DRIVE에도 연결되어 있고, 그 값은 OFF라면 바로 꺼져 버린다.
직접 해 보면 잠시 살았다가 바로 꺼져 버리는 것을 확인할 수 있다.
따라서 SDK를 사용하고자 한다면 DRIVE는 `None`으로 설정하는 것이 맞는 조치로 보인다.

Modbus로 중계하는 방안에서 본 것처럼
DRIVE를 지정하면 Tag를 DRIVE 측의 IO에 연결하는 과정을 거치게 되는데,
SDK는 Tag를 직접 읽고 쓰기 때문에 이런 연결 과정을 따로 하지 않아도 된다.

# SDK 구성

깃허브에는 소스 없이 라이브러리만 제공된다.
설명서에는 SDK가 있다는 정도만 언급될 뿐이고,
라이브러리 자체에 대한 설명은 깃허브 README에 있는 정도가 다다.

Tag는 `memory mapped file`을 통해 읽고 쓸 수 있다.
이는 다시 `Input`, `Output`, `Memory` 셋으로 나뉜다.
`Input`이 Fatory IO에서 사용자 프로그램으로 전달되는 값이고,
`Output`이 사용자 프로그램이 Fatory IO로 전달하는 값이다.
`Memory`는 일반 데이터 교환용이라고 설명하고 있지만, 딱히 어떤 쓸모가 있는지는 잘 모르겠다.

전체 `memory mapped file`은 `MemoryMap`이라는 클래스로 정의되었고,
`MemoryMap.Instance`를 통해 접근하는 싱글톤 패턴으로 되어 있다.

실제 읽고 쓰기는 `MemoryMap.Instance.Update` 할 때 이루어진다.
즉, `Update`를 호출한 시점에 `Input`의 값들은 읽혀서 복사본으로 저장되고,
앞서 설정된 값이 `Output`에 써진다.

## 읽기 예제

```c#
MemoryBit diffuseSensor = MemoryMap.Instance.GetBit(0, MemoryType.Input);
```

이렇게 한다고 `diffuseSensor`에 `Input Bit 0`의 값이 읽히는 것이 아니다.
단지 그 값을 읽어 둘 수 있는 공간이 확보되는 것이다.
실제로 값을 읽으려면 앞서 말한 것처럼 반드시 `Update`를 해 줘야 한다.

```c#
MemoryMap.Instance.Update();
```

이제 `diffuseSensor.Value`를 통해 값을 알 수 있다.

```c#
MemoryMap.Instance.Dispose();
```

마지막으로 처리 과정에서 확보된 리소스들을 해제해 준다.

## 쓰기 예제

```c#
MemoryBit rollerConveyor = MemoryMap.Instance.GetBit(0, MemoryType.Output);
rollerConveyor.Value = true;
```

읽기와 마찬가지로 이렇게 한다고 바로 값이 써지는 것이 아니다.
값을 기록해 둘 공간이 확보되고, 그 곳의 값을 바꾸는 것 뿐이다.
실제로 쓰려면 반드시 `Update`를 호출해야 한다.

```c#
MemoryMap.Instance.Update();
```

# 시범

## MX-Component

SDK가 .NET 라이브러리를 제공하므로 이를 중계하려면
MX-Component를 다루는데도 .NET 언어를 사용해야 한다.
C#을 사용할 것이다.

Visual Studio가 설치되어 있지 않으므로
[Build Tools](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=BuildTools)의 .NET SDK만으로 시범을 진행할 것이다.
좀 더 가볍게 접근한다면 [.NET SDK](https://dotnet.microsoft.com/download)만으로도 충분할 것이다.

우선 프로젝트 하나를 만든다.

```batch
dotnet new console -o acttest
```

제대로 됐다면

```batch
cd acttest
dotnet run
```

컴파일을 거친 후
화면에 'Hello World!'가 출력될 것이다.

### 32비트 프로그램

MX-Component4는 32비트 프로그램만을 지원하므로 `PlatformTarget`을 `x86`으로 바꾼다.

`acttest.csproj` 파일을 열어서

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

</Project>
```

```xml
<PlatformTarget>x86</PlatformTarget>
```

을 추가해 준다.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
    <PlatformTarget>x86</PlatformTarget>
  </PropertyGroup>

</Project>
```

### COM 라이브러리 참조 추가

MX-Component는 COM 라이브러리이므로 C#에서 사용하려면 포장(Wrap) 라이브러리의 참조가 필요하다.

역시 `acttest.csproj` 파일을 열어서

```xml
  <ItemGroup>
    <Reference Include="ActUtlType">
      <HintPath>C:\MELSEC\Act\Control\ActUtlTypeLib.dll</HintPath>
    </Reference>
    <Reference Include="ActSupportMsg">
      <HintPath>C:\MELSEC\Act\Control\ActSupportMsgLib.dll</HintPath>
    </Reference>
  </ItemGroup>
```

두 개의 참조를 추가해 준다.

### 읽고 쓰기 연습

Y1000의 값을 읽고 쓰면서 변화를 확인할 것이다.

```c#
var actctrl = new ActUtlTypeClass();
actctrl.ActLogicalStationNumber = 2;
```

`Logical Station Number`를 2로 설정한다.
미리 `Communication Setup Utility`로 설정되어 있어야 한다.

```c#
res = actctrl.Open();
// ....
res = actctrl.Close();
```

Simulator2와의 연결했다가 받는다.
마찬가지로 미리 GX Works2에서 `Start/Stop Simulation`으로 Simulator를 실행시켜 둬야 한다.
아니면 에러가 발생한다.

```
Unhandled exception. acttest.ActError: GX Simulator2 unstart error
GX Simulator2 did not start.
The corrective action is as follows:
 Start GX Simulator2.

<ErrorCode:1809001[Hex]>
```

최종적으로 `Open`과 `Close` 사이에
Y1000에 대한 코드를 넣어 읽고 쓰기를 확인한다.

```c#
var value = actctrl.GetDevice("Y1000");
Console.WriteLine("{0}", value);
actctrl.SetDevice("Y1000", 1);
value = actctrl.GetDevice("Y1000");
Console.WriteLine("{0}", value);
```
