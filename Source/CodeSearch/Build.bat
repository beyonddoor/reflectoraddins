@echo off

set FrameworkPath=%SystemRoot%\Microsoft.NET\Framework\v1.0.3705
if exist "%FrameworkPath%\csc.exe" goto :Start
set FrameworkPath=%SystemRoot%\Microsoft.NET\Framework\v1.1.4322
if exist "%FrameworkPath%\csc.exe" goto :Start
set FrameworkPath=%SystemRoot%\Microsoft.NET\Framework\v2.0.50727
if exist "%FrameworkPath%\csc.exe" goto :Start
:Start

"%FrameworkPath%\csc.exe" /nologo /target:library /out:"..\..\Build\Reflector.CodeSearch.dll" /reference:"..\..\Build\Reflector.exe" *.cs /res:Search.png,Reflector.CodeSearch.Search.png /res:Cancel.png,Reflector.CodeSearch.Cancel.png

