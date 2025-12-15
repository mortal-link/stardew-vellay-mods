using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System.Collections.Generic;

namespace PianoBlock
{
    /// <summary>Mod入口类</summary>
    public class ModEntry : Mod
    {
        public static ModEntry? Instance { get; private set; }
        public static IMonitor? ModMonitor { get; private set; }

        // 存储所有钢琴块的配置数据
        internal static Dictionary<Vector2, PianoBlockData> PianoBlocksData = new();

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            ModMonitor = Monitor;

            // 注册事件
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            Monitor.Log("Piano Block mod loaded!", LogLevel.Info);
        }

        /// <summary>加载存档时读取钢琴块数据</summary>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            PianoBlocksData = Helper.Data.ReadSaveData<Dictionary<Vector2, PianoBlockData>>("piano-blocks-data")
                ?? new Dictionary<Vector2, PianoBlockData>();
            Monitor.Log($"Loaded {PianoBlocksData.Count} piano blocks from save data", LogLevel.Debug);
        }

        /// <summary>保存游戏时写入钢琴块数据</summary>
        private void OnSaving(object? sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData("piano-blocks-data", PianoBlocksData);
            Monitor.Log($"Saved {PianoBlocksData.Count} piano blocks to save data", LogLevel.Debug);
        }

        /// <summary>每天开始时清理无效的钢琴块数据</summary>
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            CleanupInvalidBlocks();
        }

        /// <summary>清理不存在的钢琴块数据</summary>
        private void CleanupInvalidBlocks()
        {
            var locationsToCheck = Game1.locations;
            var validPositions = new HashSet<Vector2>();

            foreach (var location in locationsToCheck)
            {
                foreach (var obj in location.Objects.Values)
                {
                    if (obj.Name.Contains("Piano Block") || obj.ParentSheetIndex == 929) // 长笛块的ID，我们暂时用这个
                    {
                        validPositions.Add(obj.TileLocation);
                    }
                }
            }

            // 移除不再有效的数据
            var toRemove = new List<Vector2>();
            foreach (var pos in PianoBlocksData.Keys)
            {
                if (!validPositions.Contains(pos))
                {
                    toRemove.Add(pos);
                }
            }

            foreach (var pos in toRemove)
            {
                PianoBlocksData.Remove(pos);
            }

            if (toRemove.Count > 0)
            {
                Monitor.Log($"Cleaned up {toRemove.Count} invalid piano block entries", LogLevel.Debug);
            }
        }

        /// <summary>处理按钮点击事件</summary>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // 只处理动作按钮和右键点击
            if (!Context.IsWorldReady || (!e.Button.IsActionButton() && !e.Button.IsUseToolButton()))
                return;

            // 获取点击的位置
            Vector2 tile = e.Cursor.GrabTile;

            // 检查是否点击了钢琴块
            if (Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object? obj))
            {
                // 检查是否是钢琴块（暂时使用长笛块ID）
                if (obj.Name.Contains("Piano Block") || obj.ParentSheetIndex == 929)
                {
                    // 阻止默认行为
                    Helper.Input.Suppress(e.Button);

                    // 打开配置菜单
                    OpenPianoBlockMenu(tile);
                }
            }
        }

        /// <summary>打开钢琴块配置菜单</summary>
        private void OpenPianoBlockMenu(Vector2 position)
        {
            // 如果该位置还没有数据，创建默认数据
            if (!PianoBlocksData.ContainsKey(position))
            {
                PianoBlocksData[position] = new PianoBlockData();
            }

            // 打开菜单
            Game1.activeClickableMenu = new PianoBlockMenu(position, PianoBlocksData[position]);
        }
    }
}
