@echo off
setlocal enabledelayedexpansion

REM Prompt user for destination folder path
set /p "DEST_FOLDER=Enter the destination folder path (e.g. C:\DestinationFolder): "

REM Create destination folder if it doesn't exist
if not exist "%DEST_FOLDER%" (
    echo Creating destination folder: %DEST_FOLDER%
    mkdir "%DEST_FOLDER%"
)

REM Create system info file
echo Gathering system information...
echo Windows Device Information > "%DEST_FOLDER%\system_info.txt"
echo ========================= >> "%DEST_FOLDER%\system_info.txt"
echo. >> "%DEST_FOLDER%\system_info.txt"

REM Get OS version
systeminfo | findstr /B /C:"OS Name" /C:"OS Version" >> "%DEST_FOLDER%\system_info.txt"
echo. >> "%DEST_FOLDER%\system_info.txt"

REM Get total RAM in GB
echo RAM Information: >> "%DEST_FOLDER%\system_info.txt"
for /f "tokens=2 delims=:" %%a in ('systeminfo ^| findstr /C:"Total Physical Memory"') do (
    set "ram=%%a"
    set "ram=!ram:MB=!"
    set "ram=!ram:GB=!"
    set "ram=!ram:KB=!"
    set "ram=!ram: =!"
    set /a "ram_gb=!ram!/1024"
    echo Total RAM: !ram_gb! GB >> "%DEST_FOLDER%\system_info.txt"
)
echo. >> "%DEST_FOLDER%\system_info.txt"

REM Get Graphics Card information
echo Graphics Card Information: >> "%DEST_FOLDER%\system_info.txt"
for /f "tokens=2 delims==" %%a in ('wmic path win32_VideoController get name /value ^| findstr "="') do (
    echo GPU: %%a >> "%DEST_FOLDER%\system_info.txt"
)
for /f "tokens=2 delims==" %%a in ('wmic path win32_VideoController get adapterram /value ^| findstr "="') do (
    set /a "gpu_ram=%%a/1073741824"
    echo Memory: !gpu_ram! GB >> "%DEST_FOLDER%\system_info.txt"
)
for /f "tokens=2 delims==" %%a in ('wmic path win32_VideoController get driverversion /value ^| findstr "="') do (
    echo Driver Version: %%a >> "%DEST_FOLDER%\system_info.txt"
)

echo System information has been saved to system_info.txt

REM Export TR7600 related registry keys
echo.
echo Exporting TR7600 registry keys...
for /f "tokens=2-4 delims=/ " %%a in ('date /t') do (
    set /A "year=%%c"
    set "month=%%a"
    set "day=%%b"
)
reg export "HKEY_LOCAL_MACHINE\SOFTWARE\TRI\TR7600" "%DEST_FOLDER%\%year%%month%%day%_BACKUP.reg" /y
if errorlevel 1 (
    echo ERROR: Failed to export registry key HKEY_LOCAL_MACHINE\SOFTWARE\TRI\TR7600
) else (
    echo Registry keys exported successfully
)

REM Copy files from pre-determined locations
echo.
echo Copying files and folders...

REM Copy specific files
if exist "C:\TR7600\TriMotion.cfg" (
    xcopy "C:\TR7600\TriMotion.cfg" "%DEST_FOLDER%" /Y /I
    echo Copied: TriMotion.cfg
) else (
    echo File not found: C:\TR7600\TriMotion.cfg
)

REM Copy entire folders
echo.
if exist "C:\TR7600\Log\Calibration" (
    xcopy "C:\TR7600\Log\Calibration" "%DEST_FOLDER%\Calibration" /E /H /C /I /Y
    echo Copied: Calibration folder
) else (
    echo Folder not found: C:\TR7600\Log\Calibration
)

if exist "C:\TR7600\Log\GrayLevel" (
    xcopy "C:\TR7600\Log\GrayLevel" "%DEST_FOLDER%\GrayLevel" /E /H /C /I /Y
    echo Copied: GrayLevel folder
) else (
    echo Folder not found: C:\TR7600\Log\GrayLevel
)

if exist "C:\TRI_AXI" (
    xcopy "C:\TRI_AXI" "%DEST_FOLDER%\TRI_AXI" /E /H /C /I /Y
    echo Copied: TRI_AXI folder
) else (
    echo Folder not found: C:\TRI_AXI
)

echo.
echo Backup operation completed. Remember to copy PLC backup files.
echo Check above messages for any files or folders that were not found.
pause

