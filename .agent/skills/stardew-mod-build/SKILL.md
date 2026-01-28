---
name: Stardew Valley Mod Build
description: Build and deploy Stardew Valley mods for testing
---

# 星露谷 Mod 构建技能

帮助你构建和部署 Stardew Valley mod 进行测试。

## 构建流程

### Windows 构建
```bash
./build.bat
```

### Mac/Linux 构建
```bash
chmod +x build.sh
./build.sh
```

## 关键点

1. **构建脚本功能**：
   - 自动编译 mod
   - 打包 `[CP] Piano Block` 目录中的资源
   - 部署到 Stardew Valley 的 Mods 文件夹
   - 根据可用性使用 MSBuild 或 dotnet build

2. **项目结构**：
   - `ModEntry.cs` - 主入口点
   - `PianoBlockMenu.cs` - UI 和游戏逻辑
   - `PianoBlockData.cs` - 数据模型
   - `[CP] Piano Block/` - Content Patcher 资源
   - `manifest.json` - Mod 元数据

3. **构建要求**：
   - .NET SDK 或 MSBuild
   - 已安装 SMAPI 的 Stardew Valley
   - 构建脚本中配置正确的路径

## 常见任务

### 构建和测试
1. 修改代码
2. 运行对应的构建脚本
3. 启动 Stardew Valley 测试

### 清理构建
删除 `bin` 和 `obj` 目录后再构建，确保全新编译。

## 故障排除

- **构建失败**：检查 .NET SDK 是否已安装并在 PATH 中
- **Mod 不加载**：验证 SMAPI 已安装且 mod 文件夹存在
- **更改未生效**：确保构建脚本成功将文件复制到 Mods 文件夹

## 快速启动

桌面有 `启动星露谷.command`，双击可直接启动带 SMAPI 的游戏（不通过 Steam）。
