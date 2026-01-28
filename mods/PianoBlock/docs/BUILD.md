# 钢琴块Mod - 编译指南

本文档提供详细的编译步骤，适用于Windows、Mac和Linux系统。

## 🔧 方法一：使用命令行编译（推荐）

### 1. 安装 .NET 6.0 SDK

**Windows:**
1. 访问 https://dotnet.microsoft.com/download/dotnet/6.0
2. 下载并安装 .NET 6.0 SDK
3. 安装完成后，打开命令提示符或PowerShell验证：
   ```
   dotnet --version
   ```

**Mac:**
```bash
brew install dotnet-sdk
```

**Linux (Ubuntu/Debian):**
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 6.0
```

或使用包管理器：
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0
```

### 2. 配置星露谷路径

你需要让编译器知道星露谷的安装位置。编辑项目文件或创建环境变量：

**方法A: 自动检测（推荐）**
`Pathoschild.Stardew.ModBuildConfig` 包会自动查找常见位置：
- Windows: `C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley`
- Mac: `~/Library/Application Support/Steam/steamapps/common/Stardew Valley`
- Linux: `~/.steam/steam/steamapps/common/Stardew Valley`

**方法B: 手动指定**
如果自动检测失败，编辑 `PianoBlock.csproj`，在 `<PropertyGroup>` 中添加：
```xml
<GamePath>你的星露谷安装路径</GamePath>
```

例如：
```xml
<GamePath>C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley</GamePath>
```

### 3. 编译

在项目目录中运行：

```bash
cd /home/user/stardew-vellay-mods
dotnet build
```

或者发布版本（优化构建）：
```bash
dotnet build --configuration Release
```

### 4. 安装Mod

编译成功后，文件会自动复制到星露谷的Mods文件夹（如果配置了GamePath）。

如果没有自动复制，手动复制以下文件到 `星露谷安装路径/Mods/PianoBlock/`：
- `bin/Debug/net6.0/PianoBlock.dll`
- `manifest.json`

同时复制 `[CP] Piano Block` 整个文件夹到 `Mods/` 目录。

---

## 🎨 方法二：使用Visual Studio（Windows）

### 1. 安装Visual Studio 2022
1. 下载 [Visual Studio 2022 Community](https://visualstudio.microsoft.com/)（免费）
2. 安装时选择 **.NET 桌面开发** 工作负载

### 2. 打开项目
1. 双击 `PianoBlock.csproj` 文件
2. Visual Studio会自动加载项目

### 3. 配置游戏路径
- 右键点击项目 → 属性
- 在属性页面可以设置GamePath（如果需要）

### 4. 编译
- 按 `Ctrl + Shift + B` 或
- 点击菜单：生成 → 生成解决方案

### 5. 调试（可选）
可以配置Visual Studio直接启动游戏进行调试：
- 项目属性 → 调试
- 启动可执行文件：`星露谷路径/Stardew Valley.exe`

---

## 🦀 方法三：使用JetBrains Rider（跨平台）

### 1. 安装Rider
下载 [JetBrains Rider](https://www.jetbrains.com/rider/)（30天试用或学生免费）

### 2. 打开项目
- 文件 → 打开 → 选择 `PianoBlock.csproj`

### 3. 编译
- 点击顶部的 Build 按钮
- 或按 `Ctrl + Shift + F9` (Windows/Linux) / `⌘ + Shift + F9` (Mac)

---

## 📝 方法四：使用VS Code（轻量级）

### 1. 安装工具
1. 安装 [VS Code](https://code.visualstudio.com/)
2. 安装 .NET SDK（见方法一）
3. 在VS Code中安装扩展：
   - C# (Microsoft)
   - C# Dev Kit (Microsoft)

### 2. 打开项目
```bash
cd /home/user/stardew-vellay-mods
code .
```

### 3. 编译
打开终端（Ctrl + `）运行：
```bash
dotnet build
```

---

## 🚀 自动构建脚本

我已经为你创建了构建脚本（见下一步），使用方法：

**Linux/Mac:**
```bash
chmod +x build.sh
./build.sh
```

**Windows:**
```cmd
build.bat
```

---

## 📦 完整安装结构

编译完成后，你的Mods文件夹应该是这样的：

```
Mods/
├── PianoBlock/
│   ├── PianoBlock.dll
│   ├── manifest.json
│   └── [其他依赖文件]
└── [CP] Piano Block/
    ├── manifest.json
    └── content.json
```

---

## ❓ 常见问题

### Q: 编译时找不到Stardew Valley引用？
A: 设置GamePath或确保Steam安装在标准位置

### Q: 编译成功但游戏中看不到mod？
A: 检查：
1. SMAPI是否正确安装
2. Content Patcher是否安装
3. 查看SMAPI控制台的错误信息
4. 确认两个文件夹都复制到了Mods目录

### Q: 缺少依赖项？
A: 运行 `dotnet restore` 来恢复NuGet包

### Q: Linux上权限问题？
A: 确保对Mods文件夹有写权限

---

## 🔍 验证安装

启动游戏后，在SMAPI控制台中应该看到：
```
[Piano Block] Piano Block mod loaded!
```

游戏内检查：
1. 农业等级达到3级后
2. 去罗宾的木匠铺
3. 应该能看到"钢琴块"物品可以购买

---

## 📞 需要帮助？

如果遇到问题：
1. 检查SMAPI控制台的错误信息
2. 确认.NET SDK版本是6.0或更高
3. 查看 `bin/Debug/net6.0/` 目录是否有输出文件
4. 确认星露谷版本是1.6或更高

祝编译顺利！🎵
