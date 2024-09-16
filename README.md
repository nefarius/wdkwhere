# <img src="assets/NSS-128x128.png" align="left" />Nefarius.Tools.WDKWhere

![Requirements](https://img.shields.io/badge/Requires-.NET%208.0-blue.svg)
[![Nuget](https://img.shields.io/nuget/v/Nefarius.Tools.WDKWhere)](https://www.nuget.org/packages/Nefarius.Tools.WDKWhere/)
[![Nuget](https://img.shields.io/nuget/dt/Nefarius.Tools.WDKWhere)](https://www.nuget.org/packages/Nefarius.Tools.WDKWhere/)

CLI tool that resolves Windows SDK installation paths.

## Motivation

> *This meeting could've been an email!*

And this tool could've been a shell script 😅 `wdkwhere` was inspired by [
`vswhere`](https://github.com/microsoft/vswhere) but for locating
the [Windows SDK](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/) installation folders instead.
The SDK hosts quite a few nifty everyday developer binaries like `signtool` which by default are not easily invokable on
the Terminal or in scripts since their respective directories are all not added to e.g. the `PATH` environment variable.

Granted, this could be somewhat automated via registry queries in PowerShell or even the CMD, or Python/VBS scripts and
alike, but why not use a nice language like C# and make it a dotnet tool instead!

## Installation

```PowerShell
dotnet tool install --global Nefarius.Tools.WDKWhere
```

## Usage examples

Calling `wdkwhere` without arguments simply returns the `bin` directory for the latest local WDK version and the current
operating system architecture (most likely `X64`):

```PowerShell
PS C:\Users\Administrator> wdkwhere
C:\Program Files (x86)\Windows Kits\10\Bin\10.0.22621.0\X64
PS C:\Users\Administrator>
```

## 3rd party credits

- [CliWrap](https://github.com/Tyrrrz/CliWrap)
- [Command Line Parser Library for CLR and NetStandard](https://github.com/commandlineparser/commandline)
- [MinVer](https://github.com/adamralph/minver)
