using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MusicBlocks
{
    /// <summary>Music Blocks mod入口类 - 支持多种乐器</summary>
    public class ModEntry : Mod
    {
        public static ModEntry? Instance { get; private set; }
        public static IMonitor? ModMonitor { get; private set; }

        // 存储所有音乐方块的配置数据
        internal static Dictionary<Vector2, MusicBlockData> MusicBlocksData = new();

        // 支持的方块ID列表（长笛块和鼓块）
        private static readonly HashSet<int> SupportedBlockIds = new() { 929, 928 }; // 929=长笛块, 928=鼓块

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            ModMonitor = Monitor;

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            Monitor.Log("Music Blocks mod loaded! Supports: Piano, Guitar, Bass, Synth", LogLevel.Info);
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            MusicBlocksData = Helper.Data.ReadSaveData<Dictionary<Vector2, MusicBlockData>>("music-blocks-data")
                ?? new Dictionary<Vector2, MusicBlockData>();
            Monitor.Log($"Loaded {MusicBlocksData.Count} music blocks from save data", LogLevel.Debug);
        }

        private void OnSaving(object? sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData("music-blocks-data", MusicBlocksData);
            Monitor.Log($"Saved {MusicBlocksData.Count} music blocks to save data", LogLevel.Debug);
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            CleanupInvalidBlocks();
        }

        private void CleanupInvalidBlocks()
        {
            var locationsToCheck = Game1.locations;
            var validPositions = new HashSet<Vector2>();

            foreach (var location in locationsToCheck)
            {
                foreach (var obj in location.Objects.Values)
                {
                    if (IsMusicBlock(obj))
                    {
                        validPositions.Add(obj.TileLocation);
                    }
                }
            }

            var toRemove = new List<Vector2>();
            foreach (var pos in MusicBlocksData.Keys)
            {
                if (!validPositions.Contains(pos))
                {
                    toRemove.Add(pos);
                }
            }

            foreach (var pos in toRemove)
            {
                MusicBlocksData.Remove(pos);
            }

            if (toRemove.Count > 0)
            {
                Monitor.Log($"Cleaned up {toRemove.Count} invalid music block entries", LogLevel.Debug);
            }
        }

        /// <summary>检查对象是否是音乐方块</summary>
        private bool IsMusicBlock(StardewValley.Object obj)
        {
            return obj.Name.Contains("Music Block")
                || obj.Name.Contains("Piano Block")
                || obj.Name.Contains("Flute Block")
                || obj.Name.Contains("Drum Block")
                || SupportedBlockIds.Contains(obj.ParentSheetIndex);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || (!e.Button.IsActionButton() && !e.Button.IsUseToolButton()))
                return;

            Vector2 tile = e.Cursor.GrabTile;

            if (Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object? obj))
            {
                if (IsMusicBlock(obj))
                {
                    Helper.Input.Suppress(e.Button);
                    OpenMusicBlockMenu(tile);
                }
            }
        }

        private void OpenMusicBlockMenu(Vector2 position)
        {
            if (!MusicBlocksData.ContainsKey(position))
            {
                MusicBlocksData[position] = new MusicBlockData();
            }

            Game1.activeClickableMenu = new MusicBlockMenu(position, MusicBlocksData[position]);
        }

        /// <summary>播放指定乐器的音效</summary>
        public static void PlayInstrumentSound(InstrumentType instrument, int pitch)
        {
            string soundName = instrument switch
            {
                InstrumentType.Piano => "flute",      // 使用长笛音效模拟钢琴
                InstrumentType.Guitar => "guitar",    // 需要自定义或使用现有音效
                InstrumentType.Bass => "bass",        // 需要自定义或使用现有音效
                InstrumentType.Synth => "synth",      // 需要自定义或使用现有音效
                InstrumentType.Drum => "drumkit",     // 使用原生鼓音效
                _ => "flute"
            };

            // 尝试播放音效，如果不存在则回退到长笛音效
            try
            {
                Game1.playSound(soundName, pitch);
            }
            catch
            {
                Game1.playSound("flute", pitch);
            }
        }
    }
}
