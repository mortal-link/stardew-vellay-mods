@echo off
chcp 65001 > nul
echo =========================================
echo   钢琴块Mod - 自动构建脚本
echo =========================================
echo.

REM 检查.NET SDK
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo ❌ 错误：未找到 .NET SDK
    echo.
    echo 请先安装 .NET 6.0 SDK：
    echo   访问: https://dotnet.microsoft.com/download/dotnet/6.0
    echo.
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✓ 找到 .NET SDK: %DOTNET_VERSION%
echo.

REM 恢复NuGet包
echo 📦 恢复NuGet包...
dotnet restore
if %errorlevel% neq 0 (
    echo ❌ NuGet包恢复失败
    pause
    exit /b 1
)
echo.

REM 编译项目
echo 🔨 编译项目...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ❌ 编译失败
    pause
    exit /b 1
)
echo.

echo ✅ 编译成功！
echo.

REM 查找星露谷Mods目录
set MODS_DIR=
set "STEAM_PATH=C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods"
set "STEAM_PATH2=C:\Steam\steamapps\common\Stardew Valley\Mods"

if exist "%STEAM_PATH%" (
    set "MODS_DIR=%STEAM_PATH%"
) else if exist "%STEAM_PATH2%" (
    set "MODS_DIR=%STEAM_PATH2%"
)

if "%MODS_DIR%"=="" (
    echo ⚠️  未找到星露谷Mods目录
    echo.
    echo 📁 编译输出位置：
    echo    .\bin\Release\net6.0\
    echo.
    echo 请手动复制以下文件到你的星露谷 Mods 文件夹：
    echo    1. .\bin\Release\net6.0\ 的所有内容 → Mods\PianoBlock\
    echo    2. .\[CP] Piano Block\ → Mods\[CP] Piano Block\
    echo.
) else (
    echo 📁 找到Mods目录: %MODS_DIR%
    echo.

    set /p INSTALL="是否自动安装到Mods目录？(Y/N) "
    if /i "%INSTALL%"=="Y" (
        REM 创建目标目录
        if not exist "%MODS_DIR%\PianoBlock" mkdir "%MODS_DIR%\PianoBlock"
        if not exist "%MODS_DIR%\[CP] Piano Block" mkdir "%MODS_DIR%\[CP] Piano Block"

        REM 复制文件
        echo 📋 复制文件...
        xcopy /Y /E /I "bin\Release\net6.0\*" "%MODS_DIR%\PianoBlock\" > nul
        xcopy /Y /E /I "[CP] Piano Block\*" "%MODS_DIR%\[CP] Piano Block\" > nul

        echo ✅ 安装完成！
        echo.
        echo Mod已安装到：
        echo   - %MODS_DIR%\PianoBlock\
        echo   - %MODS_DIR%\[CP] Piano Block\
    ) else (
        echo 📁 编译输出位置：
        echo    .\bin\Release\net6.0\
    )
)

echo.
echo =========================================
echo   🎵 构建完成！
echo =========================================
echo.
echo 下一步：
echo   1. 确保已安装 SMAPI
echo   2. 确保已安装 Content Patcher
echo   3. 启动游戏
echo   4. 在木匠铺购买或制作钢琴块
echo   5. 开始创作音乐！
echo.
pause
