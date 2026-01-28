#!/bin/bash

echo "========================================="
echo "  钢琴块Mod - 自动构建脚本"
echo "========================================="
echo ""

# 检查.NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "❌ 错误：未找到 .NET SDK"
    echo ""
    echo "请先安装 .NET 6.0 SDK："
    echo "  Ubuntu/Debian: sudo apt-get install dotnet-sdk-6.0"
    echo "  Mac: brew install dotnet-sdk"
    echo "  或访问: https://dotnet.microsoft.com/download/dotnet/6.0"
    echo ""
    exit 1
fi

echo "✓ 找到 .NET SDK: $(dotnet --version)"
echo ""

# 恢复NuGet包
echo "📦 恢复NuGet包..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ NuGet包恢复失败"
    exit 1
fi
echo ""

# 编译项目
echo "🔨 编译项目..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "❌ 编译失败"
    exit 1
fi
echo ""

echo "✅ 编译成功！"
echo ""

# 查找星露谷Mods目录
MODS_DIR=""
POSSIBLE_PATHS=(
    "$HOME/.steam/steam/steamapps/common/Stardew Valley/Mods"
    "$HOME/.local/share/Steam/steamapps/common/Stardew Valley/Mods"
    "$HOME/Library/Application Support/Steam/steamapps/common/Stardew Valley/Mods"
    "/mnt/c/Program Files (x86)/Steam/steamapps/common/Stardew Valley/Mods"  # WSL
)

for path in "${POSSIBLE_PATHS[@]}"; do
    if [ -d "$path" ]; then
        MODS_DIR="$path"
        break
    fi
done

if [ -z "$MODS_DIR" ]; then
    echo "⚠️  未找到星露谷Mods目录"
    echo ""
    echo "📁 编译输出位置："
    echo "   ./bin/Release/net6.0/"
    echo ""
    echo "请手动复制以下文件到你的星露谷 Mods 文件夹："
    echo "   1. ./bin/Release/net6.0/ 的所有内容 → Mods/PianoBlock/"
    echo "   2. ./[CP] Piano Block/ → Mods/[CP] Piano Block/"
    echo ""
else
    echo "📁 找到Mods目录: $MODS_DIR"
    echo ""

    read -p "是否自动安装到Mods目录？(y/n) " -n 1 -r
    echo ""

    if [[ $REPLY =~ ^[Yy]$ ]]; then
        # 创建目标目录
        mkdir -p "$MODS_DIR/PianoBlock"
        mkdir -p "$MODS_DIR/[CP] Piano Block"

        # 复制文件
        echo "📋 复制文件..."
        cp -r bin/Release/net6.0/* "$MODS_DIR/PianoBlock/"
        cp -r "[CP] Piano Block"/* "$MODS_DIR/[CP] Piano Block/"

        echo "✅ 安装完成！"
        echo ""
        echo "Mod已安装到："
        echo "  - $MODS_DIR/PianoBlock/"
        echo "  - $MODS_DIR/[CP] Piano Block/"
    else
        echo "📁 编译输出位置："
        echo "   ./bin/Release/net6.0/"
    fi
fi

echo ""
echo "========================================="
echo "  🎵 构建完成！"
echo "========================================="
echo ""
echo "下一步："
echo "  1. 确保已安装 SMAPI"
echo "  2. 确保已安装 Content Patcher"
echo "  3. 启动游戏"
echo "  4. 在木匠铺购买或制作钢琴块"
echo "  5. 开始创作音乐！"
echo ""
