using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MusicBlocks
{
    /// <summary>Music Blocks mod入口类 - 每种乐器独立物品</summary>
    public class ModEntry : Mod
    {
        public static ModEntry? Instance { get; private set; }
        public static IMonitor? ModMonitor { get; private set; }
        public static string ModId => "MortalLink.MusicBlocks.CP";

        // 存储所有音乐方块的配置数据（key = locationName_x_y）
        internal static Dictionary<string, MusicBlockData> MusicBlocksData = new();

        // 物品ID到乐器类型的映射
        private static readonly Dictionary<string, InstrumentType> ItemIdToInstrument = new()
        {
            { "MortalLink.MusicBlocks.CP_GuitarBlock", InstrumentType.Guitar },
            { "MortalLink.MusicBlocks.CP_BassBlock", InstrumentType.Bass },
            { "MortalLink.MusicBlocks.CP_PianoBlock", InstrumentType.Piano },
            { "MortalLink.MusicBlocks.CP_SynthBlock", InstrumentType.Synth }
        };

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            ModMonitor = Monitor;

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            Monitor.Log("Music Blocks mod loaded! Items: Guitar, Bass, Piano, Synth blocks", LogLevel.Info);
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            MusicBlocksData = Helper.Data.ReadSaveData<Dictionary<string, MusicBlockData>>("music-blocks-data-v2")
                ?? new Dictionary<string, MusicBlockData>();
            Monitor.Log($"Loaded {MusicBlocksData.Count} music blocks from save data", LogLevel.Debug);
        }

        private void OnSaving(object? sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData("music-blocks-data-v2", MusicBlocksData);
            Monitor.Log($"Saved {MusicBlocksData.Count} music blocks to save data", LogLevel.Debug);
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            CleanupInvalidBlocks();
        }

        private void CleanupInvalidBlocks()
        {
            var validKeys = new HashSet<string>();

            foreach (var location in Game1.locations)
            {
                foreach (var obj in location.Objects.Values)
                {
                    if (IsMusicBlock(obj))
                    {
                        string key = GetBlockKey(location.Name, obj.TileLocation);
                        validKeys.Add(key);
                    }
                }
            }

            var toRemove = new List<string>();
            foreach (var key in MusicBlocksData.Keys)
            {
                if (!validKeys.Contains(key))
                {
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
            {
                MusicBlocksData.Remove(key);
            }

            if (toRemove.Count > 0)
            {
                Monitor.Log($"Cleaned up {toRemove.Count} invalid music block entries", LogLevel.Debug);
            }
        }

        /// <summary>生成方块的唯一key</summary>
        private static string GetBlockKey(string locationName, Vector2 tile)
        {
            return $"{locationName}_{tile.X}_{tile.Y}";
        }

        /// <summary>检查对象是否是音乐方块</summary>
        private bool IsMusicBlock(StardewValley.Object obj)
        {
            // 检查是否是我们的自定义物品
            if (obj.ItemId != null && ItemIdToInstrument.ContainsKey(obj.ItemId))
                return true;

            // 兼容旧版物品名称
            return obj.Name.Contains("GuitarBlock") ||
                   obj.Name.Contains("BassBlock") ||
                   obj.Name.Contains("PianoBlock") ||
                   obj.Name.Contains("SynthBlock");
        }

        /// <summary>获取物品的乐器类型</summary>
        private InstrumentType GetInstrumentType(StardewValley.Object obj)
        {
            // 通过 ItemId 判断
            if (obj.ItemId != null && ItemIdToInstrument.TryGetValue(obj.ItemId, out var instrument))
                return instrument;

            // 通过名称判断（兼容）
            if (obj.Name.Contains("Guitar")) return InstrumentType.Guitar;
            if (obj.Name.Contains("Bass")) return InstrumentType.Bass;
            if (obj.Name.Contains("Piano")) return InstrumentType.Piano;
            if (obj.Name.Contains("Synth")) return InstrumentType.Synth;

            return InstrumentType.Piano; // 默认
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
                    OpenMusicBlockMenu(tile, obj);
                }
            }
        }

        private void OpenMusicBlockMenu(Vector2 position, StardewValley.Object obj)
        {
            string key = GetBlockKey(Game1.currentLocation.Name, position);
            InstrumentType instrument = GetInstrumentType(obj);

            if (!MusicBlocksData.ContainsKey(key))
            {
                MusicBlocksData[key] = new MusicBlockData { Instrument = instrument };
            }
            else
            {
                // 确保乐器类型与物品匹配
                MusicBlocksData[key].Instrument = instrument;
            }

            Game1.activeClickableMenu = new MusicBlockMenu(position, MusicBlocksData[key]);
        }

        /// <summary>播放指定乐器的音效</summary>
        public static void PlayInstrumentSound(InstrumentType instrument, int pitch)
        {
            // 使用游戏内置的音效，通过音高参数调整
            // 不同乐器使用不同的基础音效
            string soundName = instrument switch
            {
                InstrumentType.Guitar => "guitarOpen",   // 吉他开弦音
                InstrumentType.Bass => "grunt",          // 低音效果
                InstrumentType.Piano => "crystal",       // 钢琴音效
                InstrumentType.Synth => "powerup",       // 电子音效
                InstrumentType.Drum => "drumkit6",       // 鼓音效
                _ => "crystal"
            };

            try
            {
                Game1.playSound(soundName, pitch);
            }
            catch
            {
                // 如果音效不存在，回退到长笛音效
                Game1.playSound("flute", pitch);
            }
        }
    }
}
