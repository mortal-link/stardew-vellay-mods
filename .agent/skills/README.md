# 技能目录

此目录包含用于星露谷 mod 开发的专业技能。

## 可用技能

### 1. stardew-mod-build
**用途**：构建和部署 Stardew Valley mod  
**使用场景**：需要编译和测试 mod 时  
**主要功能**：构建脚本自动化、部署指南

### 2. csharp-stardew-api
**用途**：使用 SMAPI 和 Stardew Valley API  
**使用场景**：开发 mod 功能、处理事件、创建 UI 时  
**主要功能**：API 模式、最佳实践、常用代码片段

## 技能工作原理

每个技能是一个包含以下内容的文件夹：
- `SKILL.md` - 主指令文件，包含：
  - YAML 前置元数据（name, description）
  - 详细的 Markdown 说明
  - 代码示例和最佳实践

## 使用技能

当 Claude 遇到与技能相关的任务时：
1. Claude 读取 `SKILL.md` 文件
2. 严格按照文档中的说明执行
3. 应用其中的模式和最佳实践

## 添加新技能

添加新技能的步骤：
1. 在 `.agent/skills/` 中创建新文件夹
2. 添加 `SKILL.md` 文件：
   ```yaml
   ---
   name: 技能名称
   description: 简短描述
   ---
   
   # 详细说明
   ```
3. 根据需要添加辅助脚本、示例或资源

## 项目背景

这是一个星露谷 mod 项目，包含：
- Piano Block 功能
- 自定义 UI 菜单
- Content Patcher 集成
- 跨平台构建支持 (Windows & Mac/Linux)

## 快速启动

桌面有 `启动星露谷.command`，双击可直接启动带 SMAPI 的游戏。
