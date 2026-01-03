using System.Collections.Generic;

namespace PianoBlock
{
    /// <summary>旋律预设，用于从JSON导入</summary>
    public class MelodyPreset
    {
        /// <summary>旋律名称</summary>
        public string Name { get; set; } = "Unnamed";

        /// <summary>音符列表，每个元素代表一个钢琴块的配置</summary>
        public List<BlockNotes> Blocks { get; set; } = new();
    }

    /// <summary>单个钢琴块的音符配置</summary>
    public class BlockNotes
    {
        /// <summary>音符列表（可以是和弦）</summary>
        public List<SimpleNote> Notes { get; set; } = new();
    }

    /// <summary>简化的音符格式，用于JSON</summary>
    public class SimpleNote
    {
        /// <summary>音符名称，如 "C4", "D#5", "A3"</summary>
        public string Note { get; set; } = "C4";

        /// <summary>延音时长（毫秒），0表示自然衰减</summary>
        public int Duration { get; set; } = 500;

        /// <summary>解析音符名称为 Pitch 和 Octave</summary>
        public (int pitch, int octave) Parse()
        {
            if (string.IsNullOrEmpty(Note) || Note.Length < 2)
                return (0, 4);

            // 找到数字开始的位置
            int digitIndex = -1;
            for (int i = 0; i < Note.Length; i++)
            {
                if (char.IsDigit(Note[i]))
                {
                    digitIndex = i;
                    break;
                }
            }

            if (digitIndex == -1)
                return (0, 4);

            string noteName = Note.Substring(0, digitIndex).ToUpper();
            string octaveStr = Note.Substring(digitIndex);

            int pitch = noteName switch
            {
                "C" => 0,
                "C#" or "DB" => 1,
                "D" => 2,
                "D#" or "EB" => 3,
                "E" => 4,
                "F" => 5,
                "F#" or "GB" => 6,
                "G" => 7,
                "G#" or "AB" => 8,
                "A" => 9,
                "A#" or "BB" => 10,
                "B" => 11,
                _ => 0
            };

            int octave = int.TryParse(octaveStr, out int o) ? o : 4;

            return (pitch, octave);
        }

        /// <summary>转换为 MusicNote</summary>
        public MusicNote ToMusicNote()
        {
            var (pitch, octave) = Parse();
            return new MusicNote
            {
                Pitch = pitch,
                Octave = octave,
                DurationMs = Duration
            };
        }
    }
}
