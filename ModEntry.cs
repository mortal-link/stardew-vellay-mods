using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PianoBlock
{
    public class ModEntry : Mod
    {
        public static ModEntry? Instance { get; private set; }
        public static IMonitor? ModMonitor { get; private set; }

        // 存储所有钢琴块的配置数据
        internal static Dictionary<Vector2, PianoBlockData> PianoBlocksData = new();

        // 记录上次触发的方块位置，避免重复触发
        private Vector2 lastTriggeredTile = Vector2.Zero;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            ModMonitor = Monitor;

            // 注册事件
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            // Initialize Audio
            AudioManager.Instance.Initialize(helper, Monitor);

            // 注册控制台命令
            helper.ConsoleCommands.Add("piano_import", "导入旋律预设。用法: piano_import <文件名>", OnImportCommand);
            helper.ConsoleCommands.Add("piano_demo", "生成小星星示例旋律（在玩家右侧放置钢琴块）", OnDemoCommand);
            helper.ConsoleCommands.Add("piano_clear", "清除当前位置所有钢琴块数据", OnClearCommand);

            Monitor.Log("Piano Block mod loaded!", LogLevel.Info);
        }

        /// <summary>控制台命令：导入旋律</summary>
        private void OnImportCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("请先加载存档！", LogLevel.Error);
                return;
            }

            if (args.Length == 0)
            {
                Monitor.Log("用法: piano_import <文件名>", LogLevel.Info);
                Monitor.Log("文件应放在 mods/PianoBlock/presets/ 目录下", LogLevel.Info);
                return;
            }

            string filename = args[0];
            if (!filename.EndsWith(".json"))
                filename += ".json";

            string filePath = Path.Combine(Helper.DirectoryPath, "presets", filename);

            if (!File.Exists(filePath))
            {
                Monitor.Log($"找不到文件: {filePath}", LogLevel.Error);
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var preset = Helper.Data.ReadJsonFile<MelodyPreset>($"presets/{filename}");

                if (preset == null || preset.Blocks.Count == 0)
                {
                    Monitor.Log("文件为空或格式错误！", LogLevel.Error);
                    return;
                }

                PlaceMelodyBlocks(preset);
                Monitor.Log($"成功导入旋律 \"{preset.Name}\"，共 {preset.Blocks.Count} 个钢琴块", LogLevel.Info);
            }
            catch (System.Exception ex)
            {
                Monitor.Log($"导入失败: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>控制台命令：生成小星星示例</summary>
        private void OnDemoCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("请先加载存档！", LogLevel.Error);
                return;
            }

            // 小星星简谱: C C G G A A G | F F E E D D C
            var twinkleStar = new MelodyPreset
            {
                Name = "小星星 (Twinkle Twinkle Little Star)",
                Blocks = new List<BlockNotes>
                {
                    new() { Notes = new() { new SimpleNote { Note = "C4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "C4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "G4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "G4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "A4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "A4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "G4", Duration = 1000 } } },
                    new() { Notes = new() { new SimpleNote { Note = "F4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "F4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "E4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "E4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "D4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "D4", Duration = 500 } } },
                    new() { Notes = new() { new SimpleNote { Note = "C4", Duration = 1000 } } },
                }
            };

            PlaceMelodyBlocks(twinkleStar);
            Monitor.Log($"已生成 \"{twinkleStar.Name}\" 旋律，共 {twinkleStar.Blocks.Count} 个钢琴块", LogLevel.Info);
            Monitor.Log("钢琴块已放置在玩家右侧，从左往右走即可演奏", LogLevel.Info);
        }

        /// <summary>控制台命令：清除钢琴块数据</summary>
        private void OnClearCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("请先加载存档！", LogLevel.Error);
                return;
            }

            int count = PianoBlocksData.Count;
            PianoBlocksData.Clear();
            Monitor.Log($"已清除 {count} 个钢琴块的配置数据", LogLevel.Info);
        }

        /// <summary>放置旋律钢琴块</summary>
        private void PlaceMelodyBlocks(MelodyPreset preset)
        {
            Vector2 startPos = Game1.player.Tile + new Vector2(1, 0); // 玩家右侧开始
            var location = Game1.currentLocation;

            for (int i = 0; i < preset.Blocks.Count; i++)
            {
                Vector2 pos = startPos + new Vector2(i, 0);

                // 创建钢琴块数据
                var blockData = new PianoBlockData
                {
                    Notes = preset.Blocks[i].Notes.Select(n => n.ToMusicNote()).ToList()
                };

                PianoBlocksData[pos] = blockData;

                // 如果位置没有物体，放置一个钢琴块
                if (!location.Objects.ContainsKey(pos))
                {
                    // 尝试创建钢琴块物体 (O) = Object, 不是 (BC) BigCraftable
                    // CP 包的 ModId 是 YourName.PianoBlock.CP
                    var pianoBlock = ItemRegistry.Create("(O)YourName.PianoBlock.CP_PianoBlock") as StardewValley.Object;
                    if (pianoBlock != null)
                    {
                        pianoBlock.TileLocation = pos;
                        location.Objects.Add(pos, pianoBlock);
                    }
                    else
                    {
                        Monitor.Log($"无法创建钢琴块物体，请确保 [CP] Piano Block 已正确加载", LogLevel.Warn);
                    }
                }
            }

            Monitor.Log($"已在 ({startPos.X}, {startPos.Y}) 放置 {preset.Blocks.Count} 个钢琴块", LogLevel.Debug);
        }

        /// <summary>每帧更新，检测玩家是否经过钢琴块</summary>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // 每帧更新音频管理器（检查需要停止的音符）
            AudioManager.Instance.Update();

            if (!Context.IsWorldReady || Game1.player == null)
                return;

            // 每4帧检测一次，减少性能消耗
            if (e.Ticks % 4 != 0)
                return;

            // 获取玩家当前位置
            Vector2 playerTile = Game1.player.Tile;

            // 检查玩家脚下和周围的方块
            Vector2[] tilesToCheck = new Vector2[]
            {
                playerTile,
                playerTile + new Vector2(0, 1), // 下方
            };

            foreach (var tile in tilesToCheck)
            {
                if (Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object? obj))
                {
                    if (IsPianoBlock(obj))
                    {
                        // 避免同一个方块重复触发
                        if (lastTriggeredTile != tile)
                        {
                            lastTriggeredTile = tile;
                            TriggerPianoBlock(tile);
                        }
                        return;
                    }
                }
            }

            // 离开方块区域，重置
            lastTriggeredTile = Vector2.Zero;
        }

        /// <summary>触发钢琴块播放</summary>
        private void TriggerPianoBlock(Vector2 position)
        {
            if (!PianoBlocksData.TryGetValue(position, out var data))
                return;

            // 同时播放所有音符（和弦），每个音符有自己的延音时长
            foreach (var note in data.Notes)
            {
                PlayNote(note, note.DurationMs);
            }
        }

        /// <summary>播放单个音符</summary>
        /// <param name="note">音符</param>
        /// <param name="durationMs">延音时长（毫秒），0表示自然衰减</param>
        public static void PlayNote(MusicNote note, int durationMs = 0)
        {
            try
            {
                // 使用自定义音频管理器播放
                AudioManager.Instance.PlayPianoNote(note.Pitch, note.Octave, durationMs);
            }
            catch (System.Exception ex)
            {
                ModMonitor?.Log($"Error playing note: {ex.Message}", LogLevel.Error);
                // Fallback
                try
                {
                    int pitch = note.GetGamePitch();
                    Game1.playSound("flute", pitch);
                }
                catch
                {
                    Game1.playSound("flute");
                }
            }
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
                    if (IsPianoBlock(obj))
                    {
                        validPositions.Add(obj.TileLocation);
                    }
                }
            }

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
            if (!Context.IsWorldReady || (!e.Button.IsActionButton() && !e.Button.IsUseToolButton()))
                return;

            // 如果菜单已经打开，不进行交互，防止点击UI时触发方块逻辑
            if (Game1.activeClickableMenu != null)
                return;

            Vector2 tile = e.Cursor.GrabTile;

            if (Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object? obj))
            {
                if (IsPianoBlock(obj))
                {
                    Helper.Input.Suppress(e.Button);
                    OpenPianoBlockMenu(tile);
                }
            }
        }

        /// <summary>检查物品是否是钢琴块</summary>
        private bool IsPianoBlock(StardewValley.Object obj)
        {
            return obj.ItemId?.Contains("PianoBlock") == true
                || obj.Name?.Contains("PianoBlock") == true;
        }

        /// <summary>打开钢琴块配置菜单</summary>
        private void OpenPianoBlockMenu(Vector2 position)
        {
            if (!PianoBlocksData.ContainsKey(position))
            {
                PianoBlocksData[position] = new PianoBlockData();
            }

            Game1.activeClickableMenu = new PianoBlockMenu(position, PianoBlocksData[position]);
        }
    }
}
