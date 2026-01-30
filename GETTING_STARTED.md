# 🎮 完整入门指南 - 从零开始

这个指南会带你从完全没有mod经验，到成功在游戏中使用钢琴块mod。

## 📋 前置准备清单

在开始之前，你需要：
- ✅ 星露谷物语游戏（Steam、GOG或其他平台）
- ✅ 电脑（Windows、Mac或Linux）
- ✅ 基本的文件管理能力

## 第一步：找到你的游戏安装位置

### Windows (Steam)
默认位置：
```
C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley
```

如何找到：
1. 打开Steam
2. 右键点击"星露谷物语"
3. 属性 → 本地文件 → 浏览本地文件

### Mac (Steam)
```
~/Library/Application Support/Steam/steamapps/common/Stardew Valley
```

### Linux (Steam)
```
~/.steam/steam/steamapps/common/Stardew Valley
```

**记住这个位置！** 后面会用到。

---

## 第二步：安装SMAPI（Mod加载器）

SMAPI是让mod能够运行的基础框架，就像是mod的"管家"。

### 1. 下载SMAPI

访问官方网站：https://smapi.io/

点击 **Download SMAPI** 按钮

### 2. 安装SMAPI

#### Windows用户：
1. 解压下载的zip文件
2. 双击运行 `install on Windows.bat`
3. 按照提示操作（通常直接按回车即可）
4. 等待安装完成

#### Mac用户：
1. 解压下载的zip文件
2. 双击运行 `install on Mac.command`
3. 如果提示权限问题，右键 → 打开
4. 按照提示操作

#### Linux用户：
1. 解压下载的zip文件
2. 打开终端，进入SMAPI文件夹
3. 运行 `./install on Linux.sh`
4. 按照提示操作

### 3. 验证SMAPI安装

安装完成后，你应该能看到：
- Windows: 桌面上有 `Stardew Valley.exe` 和 `StardewModdingAPI.exe`
- Mac/Linux: 游戏目录中有SMAPI启动器

---

## 第三步：安装Content Patcher

Content Patcher是让钢琴块mod能够添加游戏内容的依赖mod。

### 1. 下载Content Patcher

访问：https://www.nexusmods.com/stardewvalley/mods/1915

点击 **Files** 选项卡 → 下载最新版本

（可能需要注册Nexus账号，免费的）

### 2. 安装Content Patcher

1. 解压下载的zip文件
2. 找到你的游戏安装位置
3. 进入 `Mods` 文件夹（如果没有就创建一个）
4. 将解压出来的 `ContentPatcher` 文件夹复制到 `Mods` 文件夹

现在你的结构应该是：
```
Stardew Valley/
└── Mods/
    └── ContentPatcher/
        ├── manifest.json
        └── ...其他文件
```

---

## 第四步：编译钢琴块Mod

### 选项A：使用自动构建脚本（推荐）

#### 1. 安装.NET SDK

**Windows:**
- 访问 https://dotnet.microsoft.com/download/dotnet/6.0
- 下载 ".NET 6.0 SDK" (不是Runtime)
- 安装后重启电脑

**Mac:**
```bash
brew install dotnet-sdk
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0
```

验证安装：
```bash
dotnet --version
```
应该显示 `6.0.x` 或更高版本

#### 2. 运行构建脚本

**Windows:**
1. 在项目文件夹中双击 `build.bat`
2. 等待编译完成
3. 当询问是否安装时，输入 `Y` 并回车

**Mac/Linux:**
1. 打开终端
2. 进入项目目录：
   ```bash
   cd /path/to/stardew-vellay-mods
   ```
3. 运行：
   ```bash
   chmod +x build.sh
   ./build.sh
   ```
4. 当询问是否安装时，输入 `y` 并回车

### 选项B：手动安装（如果自动脚本不工作）

1. 编译项目：
   ```bash
   cd /path/to/stardew-vellay-mods
   dotnet build --configuration Release
   ```

2. 复制文件到Mods目录：
   - 复制 `bin/Release/net6.0/` 的所有内容到 `星露谷/Mods/PianoBlock/`
   - 复制 `[CP] Piano Block/` 整个文件夹到 `星露谷/Mods/`

最终结构应该是：
```
Stardew Valley/
└── Mods/
    ├── ContentPatcher/
    ├── PianoBlock/
    │   ├── PianoBlock.dll
    │   ├── manifest.json
    │   └── ...其他文件
    └── [CP] Piano Block/
        ├── manifest.json
        └── content.json
```

---

## 第五步：启动游戏！

### 重要：必须通过SMAPI启动

**不要**使用Steam或游戏原本的启动方式！

### Windows启动方式：

**方法1: 双击启动（推荐）**
- 找到游戏目录
- 双击 `StardewModdingAPI.exe`

**方法2: 通过Steam启动**
1. 打开Steam
2. 右键"星露谷物语" → 属性
3. 启动选项中输入：
   ```
   "%command%/../StardewModdingAPI.exe" %command%
   ```
4. 然后就可以从Steam启动了

### Mac启动方式：
- 打开游戏目录中的 `StardewValley.app`
- SMAPI会自动接管

### Linux启动方式：
```bash
cd "~/.steam/steam/steamapps/common/Stardew Valley"
./StardewModdingAPI
```

### 启动后检查

当游戏启动时，你会看到一个**黑色的控制台窗口**（SMAPI控制台），这是正常的！

在控制台中查找：
```
[SMAPI] Loaded 3 mods:
[SMAPI]    Content Patcher 2.x.x by Pathoschild
[SMAPI]    [CP] Piano Block 1.0.0 by YourName
[SMAPI]    Piano Block 1.0.0 by YourName
```

如果看到这些，说明mod成功加载了！🎉

---

## 第六步：在游戏中使用钢琴块

### 1. 创建或加载存档

进入你的农场

### 2. 获取钢琴块

**方法A: 购买**
1. 提升农业等级到3级（种植和收获作物）
2. 去木匠铺（罗宾的房子）
3. 在商店中应该能看到"钢琴块"（500金币）

**方法B: 制作**
1. 达到农业等级3后自动学会配方
2. 准备材料：10个木材 + 1个铜锭
3. 在制作菜单中制作

**临时测试方法（开发用）：**
如果你想快速测试，可以使用SMAPI控制台作弊：
1. 在游戏中按 `~` 键（或其他键，根据你的键盘）打开控制台
2. 输入：`player_add name {{ModId}}_PianoBlock`
3. 钢琴块会直接添加到背包

### 3. 放置钢琴块

- 在背包中选择钢琴块
- 在农场或室内找个空地
- 右键点击放置

### 4. 配置音乐

- 右键点击已放置的钢琴块
- 会弹出配置菜单

**配置界面说明：**
- **左侧列表**：显示你配置的音符序列
- **↑/↓ 按钮**：调整音高（半音，C→C#→D...）
- **8↑/8↓ 按钮**：调整八度（低音↔高音）
- **X 按钮**：删除该音符
- **添加 按钮**：添加新音符到序列
- **播放 按钮**：试听当前配置的音乐
- **曲速控制**：右上角的箭头调整播放速度
- **OK 按钮**：保存并关闭

### 5. 创作音乐示例

**简单示例 - Do Re Mi：**
1. 第一个音符：C4（默认）
2. 点击"添加"
3. 第二个音符：点击↑两次变成D4
4. 点击"添加"
5. 第三个音符：点击↑两次变成E4
6. 点击"播放"试听
7. 点击"OK"保存

**和弦示例 - C大三和弦：**
1. 第一个音符：C4
2. 添加：E4（↑4次）
3. 添加：G4（↑7次）
4. 将曲速调到最快（100ms）
5. 这样三个音几乎同时播放，形成和弦

### 6. 组合多个方块

- 放置多个钢琴块
- 每个配置不同的音符或旋律
- 按顺序点击它们来演奏完整曲子
- 或者制作成"钢琴键盘"布局

---

## 🔧 故障排除

### 游戏启动时SMAPI报错

**"Piano Block requires Content Patcher"**
- 没有安装Content Patcher
- 或Content Patcher版本太老
- 解决：重新下载并安装最新版Content Patcher

**"Failed to load manifest"**
- manifest.json文件损坏
- 解决：重新编译mod或重新复制文件

### 游戏中看不到钢琴块物品

**检查清单：**
1. ✅ 农业等级是否达到3级？
2. ✅ 是否去的是罗宾的木匠铺（不是Pierre的杂货店）？
3. ✅ Content Patcher是否正确加载？
4. ✅ [CP] Piano Block文件夹是否在Mods目录？

**临时解决：**
使用SMAPI控制台直接添加物品测试

### 点击钢琴块没有反应

**可能原因：**
1. 点击方式不对（应该右键）
2. Mod代码问题
3. 检查SMAPI控制台有无错误信息

### SMAPI控制台显示红色错误

- 截图错误信息
- 检查mod文件是否完整
- 确认.NET SDK版本正确
- 尝试重新编译

---

## 📊 验证安装清单

在开始游戏前，确认：

- [ ] SMAPI已安装（有StardewModdingAPI.exe）
- [ ] Content Patcher在Mods文件夹中
- [ ] PianoBlock文件夹在Mods中，包含.dll文件
- [ ] [CP] Piano Block文件夹在Mods中
- [ ] 从SMAPI启动游戏（不是原版启动器）
- [ ] SMAPI控制台显示两个mod都加载成功
- [ ] 农业等级达到3级

全部打勾后，去木匠铺就能看到钢琴块了！

---

## 🎵 创作建议

### 音符对照表

| 音名 | ↑次数 | 说明 |
|------|-------|------|
| C    | 0     | Do   |
| C#   | 1     | Do#  |
| D    | 2     | Re   |
| D#   | 3     | Re#  |
| E    | 4     | Mi   |
| F    | 5     | Fa   |
| F#   | 6     | Fa#  |
| G    | 7     | Sol  |
| G#   | 8     | Sol# |
| A    | 9     | La   |
| A#   | 10    | La#  |
| B    | 11    | Si   |

### 简单曲子示例

**小星星（Twinkle Twinkle）：**
```
C C G G | A A G - | F F E E | D D C -
Do Do Sol Sol | La La Sol - | Fa Fa Mi Mi | Re Re Do -
```

每个 `|` 可以是一个方块，或者全部放在一个方块中。

---

## ❓ 还有问题？

1. **查看SMAPI控制台** - 大多数问题会有明确的错误提示
2. **检查文件结构** - 确保所有文件在正确位置
3. **重新编译** - 删除bin文件夹后重新运行build脚本
4. **查看日志** - SMAPI会在游戏目录生成 `SMAPI-latest.log`

---

## 🎉 成功标志

当你能做到以下几点，说明一切正常：

1. ✅ SMAPI启动游戏时看到mod加载信息
2. ✅ 在木匠铺能看到并购买钢琴块
3. ✅ 放置钢琴块后右键能打开配置界面
4. ✅ 配置音符后点击"播放"能听到声音
5. ✅ 保存后再次点击方块会播放配置的音乐

**祝你在星露谷创作出美妙的音乐！** 🎶

---

需要更多帮助？查看项目中的其他文档：
- **QUICKSTART.md** - 快速开始（开发者）
- **BUILD.md** - 详细编译指南
- **README.md** - 功能说明
