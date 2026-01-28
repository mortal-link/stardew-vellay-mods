# 🚀 快速开始指南

5分钟内从源码到游戏中使用钢琴块！

## 第一步：安装.NET SDK（一次性设置）

### Windows
1. 访问 https://dotnet.microsoft.com/download/dotnet/6.0
2. 下载并安装 .NET 6.0 SDK（选择Windows版本）
3. 完成后重启终端

### Mac
```bash
brew install dotnet-sdk
```

### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0
```

## 第二步：编译mod

**打开终端/命令提示符，进入项目目录：**

### Windows
```cmd
cd C:\path\to\stardew-vellay-mods
build.bat
```

### Linux/Mac
```bash
cd /path/to/stardew-vellay-mods
chmod +x build.sh
./build.sh
```

脚本会提示你是否自动安装到Mods文件夹，选择 `Y` 即可。

## 第三步：安装依赖mod（如果还没安装）

1. **安装SMAPI**
   - 访问 https://smapi.io/
   - 下载并运行安装器
   - 选择你的星露谷安装位置

2. **安装Content Patcher**
   - 访问 https://www.nexusmods.com/stardewvalley/mods/1915
   - 下载最新版本
   - 解压到 `星露谷/Mods/` 文件夹

## 第四步：启动游戏

1. 运行SMAPI（不是游戏的原版启动器）
2. 在SMAPI控制台中应该看到：
   ```
   [Piano Block] Piano Block mod loaded!
   ```

## 第五步：在游戏中使用

1. **获取钢琴块**
   - 方法1：提升农业等级到3级，去罗宾的木匠铺购买（500金币）
   - 方法2：学习配方后制作（10木材 + 1铜锭）

2. **放置并配置**
   - 放置钢琴块在地图上
   - 右键点击打开配置界面

3. **创作音乐**
   - 点击"添加"按钮添加音符
   - 使用 ↑/↓ 调整音高
   - 使用 8↑/8↓ 调整八度
   - 调整曲速
   - 点击"播放"测试

4. **开始创作！**
   - 放置多个钢琴块
   - 每个块配置不同的音符
   - 组合成你的音乐作品

## 常见问题快速解答

### ❓ 编译失败？
- 确认已正确安装.NET 6.0 SDK
- 运行 `dotnet --version` 检查版本
- 尝试手动运行：
  ```bash
  dotnet restore
  dotnet build
  ```

### ❓ 游戏中看不到mod？
1. 确认SMAPI启动时显示了mod加载信息
2. 检查是否安装了Content Patcher
3. 确认两个文件夹都在Mods目录：
   - `Mods/PianoBlock/`
   - `Mods/[CP] Piano Block/`

### ❓ 找不到钢琴块物品？
- 需要农业等级达到3级
- 去木匠铺（罗宾的店）
- 应该能看到"钢琴块"可购买

### ❓ 想要修改代码？
1. 编辑 `.cs` 文件
2. 重新运行构建脚本
3. 重启游戏测试

## 项目结构

```
stardew-vellay-mods/
├── ModEntry.cs              # Mod主入口
├── PianoBlockData.cs        # 数据模型
├── PianoBlockMenu.cs        # UI界面
├── PianoBlock.csproj        # 项目配置
├── manifest.json            # Mod元数据
├── [CP] Piano Block/        # Content Patcher内容
│   ├── manifest.json
│   └── content.json
├── build.sh                 # Linux/Mac构建脚本
├── build.bat                # Windows构建脚本
├── README.md                # 完整说明
├── BUILD.md                 # 详细编译指南
└── QUICKSTART.md           # 本文件
```

## 下一步

- 📖 阅读 [README.md](README.md) 了解完整功能
- 🔧 阅读 [BUILD.md](BUILD.md) 了解编译选项
- 🎵 开始创作你的星露谷音乐！

---

**遇到问题？** 检查SMAPI控制台的错误信息，那里通常会有详细的提示。
