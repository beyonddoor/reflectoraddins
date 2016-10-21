set FrameworkPath=%SystemRoot%\Microsoft.NET\Framework\v1.0.3705
if exist "%FrameworkPath%\csc.exe" goto :Start
set FrameworkPath=%SystemRoot%\Microsoft.NET\Framework\v1.1.4322
if exist "%FrameworkPath%\csc.exe" goto :Start
set FrameworkPath=%SystemRoot%\Microsoft.NET\Framework\v2.0.50727
if exist "%FrameworkPath%\csc.exe" goto :Start
:Start

"%FrameworkPath%\csc.exe" /nologo /target:exe /out:"..\..\Build\Reflector.CodeModelExample.exe" /reference:"..\..\Build\Reflector.exe" *.cs