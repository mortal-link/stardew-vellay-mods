---
name: C# Stardew Valley API
description: Working with Stardew Valley and SMAPI APIs for mod development
---

# C# 星露谷 API 技能

提供使用 SMAPI 和 Stardew Valley API 的开发指南。

## 核心 SMAPI 概念

### Mod 入口点
每个 SMAPI mod 必须有一个继承 `StardewModdingAPI.Mod` 的类：

```csharp
public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        // 初始化代码
    }
}
```

### 常用 API

1. **事件处理**：
   ```csharp
   helper.Events.GameLoop.GameLaunched += OnGameLaunched;
   helper.Events.Input.ButtonPressed += OnButtonPressed;
   ```

2. **游戏状态访问**：
   ```csharp
   Game1.player          // 当前玩家
   Game1.activeClickableMenu  // 当前菜单
   Game1.currentLocation     // 当前位置
   ```

3. **自定义菜单**：
   ```csharp
   public class CustomMenu : IClickableMenu
   {
       public CustomMenu() : base(x, y, width, height) { }
       
       public override void draw(SpriteBatch b)
       {
           // 绘制代码
       }
   }
   ```

## Content Patcher 集成

在 `[CP] Piano Block/` 中使用 Content Patcher 资源：
- `content.json` 定义资源补丁
- 资源在游戏启动时加载
- 使用 token 实现动态内容

## 最佳实践

1. **性能优化**：
   - 缓存频繁访问的数据
   - 避免在 draw 循环中执行昂贵操作
   - 使用事件代替轮询

2. **兼容性**：
   - 检查 SMAPI 版本要求
   - 优雅处理缺失的依赖
   - 使用语义版本控制

3. **调试**：
   - 使用 `Monitor.Log()` 记录日志
   - 检查 SMAPI 控制台的错误信息
   - 使用 SMAPI 的错误处理机制

## 资源链接

- [SMAPI 文档](https://stardewvalleywiki.com/Modding:Index)
- [星露谷 Wiki](https://stardewvalleywiki.com/)
- [Content Patcher 指南](https://github.com/Pathoschild/StardewMods/tree/develop/ContentPatcher)
