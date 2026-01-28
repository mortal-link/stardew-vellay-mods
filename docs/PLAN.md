# Stardew Valley Mods 项目规划

## 项目概述

本项目包含多个 Stardew Valley 相关的模组和工具，主要围绕**音乐方块**和**地图扩展**功能。

---

## 项目结构

```
stardew-vellay-mods/
├── mods/                           # 所有游戏模组
│   ├── MusicBlocks/                # 音乐方块模组
│   └── BigMapMod/                  # 大地图模组
├── tools/                          # 共享工具和库
│   └── midi-parser/                # MIDI 解析库（可被多个项目复用）
├── web/                            # Web 应用
│   └── midi-to-save/               # MIDI 转存档网站
├── docs/                           # 文档
│   └── PLAN.md                     # 本文件
└── .gitignore
```

---

## 模组一：MusicBlocks（音乐方块模组）

### 目标

扩展游戏原生的鼓块（Drum Block），添加更多乐器类型的音乐方块。

### 功能规划

| 方块类型 | 说明 | 优先级 |
|---------|------|-------|
| Guitar Block（吉他块） | 弹拨弦乐音色 | P0 |
| Bass Block（贝斯块） | 低音弦乐音色 | P0 |
| Piano Block（钢琴块） | 键盘乐器音色 | P1 |
| Synth Block（合成器块） | 电子音色 | P2 |

### 技术方案

```
mods/MusicBlocks/
├── MusicBlocks.csproj              # C# 项目文件
├── manifest.json                   # SMAPI 模组清单
├── ModEntry.cs                     # 模组入口
├── assets/                         # 资源文件
│   ├── sprites/                    # 方块贴图
│   │   ├── guitar_block.png
│   │   ├── bass_block.png
│   │   └── ...
│   └── sounds/                     # 音效文件
│       ├── guitar/
│       ├── bass/
│       └── ...
├── src/
│   ├── Blocks/                     # 方块定义
│   │   ├── GuitarBlock.cs
│   │   ├── BassBlock.cs
│   │   └── MusicBlockBase.cs       # 基类
│   ├── Audio/                      # 音频处理
│   │   └── SoundManager.cs
│   └── Data/                       # 数据模型
│       └── NoteData.cs
└── i18n/                           # 国际化
    ├── default.json
    └── zh.json
```

### 核心机制

1. **音高控制**：参考原生鼓块，支持 24 个音高（2 个八度）
2. **触发方式**：玩家交互 / 红石信号（如果有相关 mod）/ 定时器
3. **音色切换**：通过右键菜单或配置文件选择不同音色变体

---

## 模组二：BigMapMod（大地图模组）

### 目标

创建一个超大型音乐演奏地图，用于放置大量音乐方块，实现复杂的音乐演奏。

### 功能规划

| 功能 | 说明 | 优先级 |
|-----|------|-------|
| 大型空白地图 | 提供足够空间放置音乐方块 | P0 |
| 网格系统 | 方便对齐和定位方块 | P1 |
| 分区管理 | 将地图分为不同乐器区域 | P1 |
| 快速传送 | 在不同区域间快速移动 | P2 |

### 技术方案

```
mods/BigMapMod/
├── BigMapMod.csproj
├── manifest.json
├── ModEntry.cs
├── assets/
│   └── maps/
│       ├── MusicStudio.tmx         # Tiled 地图文件
│       ├── MusicStudio.png         # 地图贴图
│       └── ...
├── src/
│   ├── MapLoader.cs                # 地图加载器
│   ├── GridSystem.cs               # 网格系统
│   └── TeleportManager.cs          # 传送管理
└── i18n/
    ├── default.json
    └── zh.json
```

### 地图设计建议

```
+------------------------------------------+
|              音乐工作室地图                 |
+------------------------------------------+
|  [鼓区]  |  [贝斯区]  |  [吉他区]  | [钢琴] |
|         |           |           |        |
+---------+-----------+-----------+--------+
|                                          |
|              [主演奏区]                    |
|         (可放置复杂编排)                   |
|                                          |
+------------------------------------------+
|  [控制室]  |  [仓库]  |  [传送点]          |
+------------------------------------------+
```

---

## 模组依赖关系

```
BigMapMod (可选依赖) ──────> MusicBlocks
     │                          │
     │                          │
     └────────> SMAPI <─────────┘
```

- **MusicBlocks**：独立模组，可单独使用
- **BigMapMod**：可选依赖 MusicBlocks，没有也能运行，但配合使用效果更好

### manifest.json 依赖配置示例

```json
// MusicBlocks/manifest.json
{
  "Name": "Music Blocks",
  "UniqueID": "MortalLink.MusicBlocks",
  "Version": "1.0.0",
  "MinimumApiVersion": "3.0.0",
  "Description": "Adds guitar, bass and other music blocks to the game",
  "Dependencies": []
}

// BigMapMod/manifest.json
{
  "Name": "Big Map Mod",
  "UniqueID": "MortalLink.BigMapMod",
  "Version": "1.0.0",
  "MinimumApiVersion": "3.0.0",
  "Description": "A large map designed for music creation",
  "Dependencies": [
    {
      "UniqueID": "MortalLink.MusicBlocks",
      "IsRequired": false  // 可选依赖
    }
  ]
}
```

---

## Web 应用：MIDI to Save Converter

### 目标

提供一个网站，将 MIDI 文件转换为游戏存档数据，自动在地图上布置音乐方块。

### 功能规划

| 功能 | 说明 | 优先级 |
|-----|------|-------|
| MIDI 上传解析 | 解析 MIDI 文件的音轨、音符数据 | P0 |
| 音轨预览 | 可视化显示各音轨 | P1 |
| 乐器映射 | 将 MIDI 乐器映射到游戏方块类型 | P0 |
| 地图布局生成 | 计算方块在地图上的位置 | P0 |
| 存档导出 | 生成可导入游戏的存档文件 | P0 |

### 技术栈建议

```
web/midi-to-save/
├── package.json
├── src/
│   ├── components/                 # UI 组件
│   │   ├── MidiUploader.tsx
│   │   ├── TrackViewer.tsx
│   │   ├── InstrumentMapper.tsx
│   │   └── MapPreview.tsx
│   ├── lib/
│   │   ├── midi-parser.ts          # MIDI 解析
│   │   ├── note-mapper.ts          # 音符映射
│   │   ├── layout-generator.ts     # 布局生成
│   │   └── save-exporter.ts        # 存档导出
│   └── pages/
│       └── index.tsx
├── public/
└── ...
```

### 数据流程

```
MIDI 文件
    │
    ▼
┌─────────────┐
│ MIDI Parser │  ──> 解析音符、时间、音轨
└─────────────┘
    │
    ▼
┌─────────────────┐
│ Instrument Map  │  ──> MIDI 乐器 → 游戏方块类型
└─────────────────┘
    │
    ▼
┌──────────────────┐
│ Layout Generator │  ──> 计算方块位置（考虑时序）
└──────────────────┘
    │
    ▼
┌───────────────┐
│ Save Exporter │  ──> 生成游戏存档 JSON
└───────────────┘
    │
    ▼
游戏存档文件（可导入）
```

### MIDI 到方块的映射逻辑

```
MIDI Note Event:
{
  track: 0,
  channel: 0,
  note: 60,        // C4 (中央 C)
  velocity: 100,
  time: 480        // ticks
}
    │
    ▼
Game Block:
{
  type: "GuitarBlock",
  x: 10,           // 根据 time 计算
  y: 5,            // 根据 track 分配
  pitch: 12,       // 根据 note 映射 (0-23)
  enabled: true
}
```

---

## 共享工具库

### tools/midi-parser

可被 Web 应用和可能的命令行工具复用的 MIDI 解析库。

```
tools/midi-parser/
├── package.json
├── src/
│   ├── index.ts
│   ├── parser.ts           # MIDI 文件解析
│   ├── types.ts            # 类型定义
│   └── utils.ts            # 工具函数
└── tests/
    └── parser.test.ts
```

---

## 开发路线图

### Phase 1: 基础设施（当前阶段）

- [x] 创建项目仓库
- [x] 规划项目结构
- [ ] 搭建开发环境
- [ ] 创建基础项目骨架

### Phase 2: MusicBlocks 模组

- [ ] 研究 SMAPI 模组开发
- [ ] 研究原生鼓块实现
- [ ] 实现 GuitarBlock
- [ ] 实现 BassBlock
- [ ] 添加音效资源
- [ ] 测试和优化

### Phase 3: BigMapMod

- [ ] 学习 Tiled 地图编辑
- [ ] 设计地图布局
- [ ] 实现地图加载
- [ ] 添加网格系统
- [ ] 实现快速传送

### Phase 4: MIDI Converter Web

- [ ] 搭建 Web 项目
- [ ] 实现 MIDI 解析
- [ ] 实现音符映射
- [ ] 实现布局生成
- [ ] 实现存档导出
- [ ] UI 界面开发

### Phase 5: 整合优化

- [ ] 完善文档
- [ ] 发布模组
- [ ] 收集反馈
- [ ] 持续迭代

---

## 参考资源

### SMAPI 模组开发

- [SMAPI 官方文档](https://stardewvalleywiki.com/Modding:Modder_Guide)
- [SMAPI GitHub](https://github.com/Pathoschild/SMAPI)

### Stardew Valley 数据

- [Stardew Valley Wiki](https://stardewvalleywiki.com/)
- [游戏数据格式](https://stardewvalleywiki.com/Modding:Editing_XNB_files)

### MIDI 处理

- [MIDI.js](https://github.com/mudcube/MIDI.js)
- [Tone.js](https://tonejs.github.io/)

---

## 备注

- 本文档会随项目进展持续更新
- 具体实现细节可能根据实际情况调整
- 欢迎提出建议和改进意见
