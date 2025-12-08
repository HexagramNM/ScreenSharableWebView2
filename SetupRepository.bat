@echo off

cd %~dp0external\NAudio

dotnet restore

for /f "usebackq delims=" %%A in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set MSBUILD_EXE=%%A

"%MSBUILD_EXE%" NAudio.sln /m /p:Configuration=Release;Platform="Any CPU"

if not exist "%~dp0LocalNAudio" (
    mkdir "%~dp0LocalNAudio"
)

copy "%~dp0external\NAudio\NAudio\bin\Release\NAudio.2.1.0-beta.1.nupkg" "%~dp0LocalNAudio"
copy "%~dp0external\NAudio\NAudio.Asio\bin\Release\NAudio.Asio.2.1.0-beta.1.nupkg" "%~dp0LocalNAudio"
copy "%~dp0external\NAudio\NAudio.Core\bin\Release\NAudio.Core.2.1.0-beta.1.nupkg" "%~dp0LocalNAudio"
copy "%~dp0external\NAudio\NAudio.Extras\bin\Release\NAudio.Extras.2.1.0-beta.1.nupkg" "%~dp0LocalNAudio"
copy "%~dp0external\NAudio\NAudio.Midi\bin\Release\NAudio.Midi.2.1.0-beta.1.nupkg" "%~dp0LocalNAudio"
copy "%~dp0external\NAudio\NAudio.Uap\bin\Release\NAudio.Uap.2.1.0-beta.1.nupkg" "%~dp0LocalNAudio"
copy "%~dp0external\NAudio\NAudio.Wasapi\bin\Release\NAudio.Wasapi.2.1.0-beta.1.nupkg" "%~dp0LocalNAudio"
copy "%~dp0external\NAudio\NAudio.WinForms\bin\Release\NAudio.WinForms.2.1.0-beta.1.nupkg" "%~dp0LocalNAudio"
copy "%~dp0external\NAudio\NAudio.WinMM\bin\Release\NAudio.WinMM.2.1.0-beta.1.nupkg" "%~dp0LocalNAudio"

cd "%~dp0"

dotnet restore
