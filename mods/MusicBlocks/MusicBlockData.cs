using System.Collections.Generic;

namespace MusicBlocks
{
    /// <summary>乐器类型</summary>
    public enum InstrumentType
    {
        Piano = 0,
        Guitar = 1,
        Bass = 2,
        Synth = 3,
        Drum = 4  // 原生支持，这里作为兼容选项
    }

    /// <summary>音乐方块的配置数据</summary>
    public class MusicBlockData
    {
        /// <summary>乐器类型</summary>
        public InstrumentType Instrument { get; set; } = InstrumentType.Piano;

        /// <summary>音符序列</summary>
        public List<MusicNote> Notes { get; set; } = new() { new MusicNote() };

        /// <summary>播放速度（毫秒/音符）</summary>
        public int Tempo { get; set; } = 500;

        /// <summary>是否循环播放</summary>
        public bool Loop { get; set; } = false;

        /// <summary>音量（0-100）</summary>
        public int Volume { get; set; } = 100;

        /// <summary>当前正在播放的音符索引</summary>
        public int CurrentNoteIndex { get; set; } = 0;

        /// <summary>上次播放时间</summary>
        public long LastPlayTime { get; set; } = 0;

        /// <summary>获取乐器显示名称</summary>
        public string GetInstrumentName()
        {
            return Instrument switch
            {
                InstrumentType.Piano => "Piano",
                InstrumentType.Guitar => "Guitar",
                InstrumentType.Bass => "Bass",
                InstrumentType.Synth => "Synth",
                InstrumentType.Drum => "Drum",
                _ => "Unknown"
            };
        }

        /// <summary>获取乐器中文名称</summary>
        public string GetInstrumentNameCN()
        {
            return Instrument switch
            {
                InstrumentType.Piano => "钢琴",
                InstrumentType.Guitar => "吉他",
                InstrumentType.Bass => "贝斯",
                InstrumentType.Synth => "合成器",
                InstrumentType.Drum => "鼓",
                _ => "未知"
            };
        }
    }

    /// <summary>单个音符</summary>
    public class MusicNote
    {
        /// <summary>音高（0-11对应C到B）</summary>
        public int Pitch { get; set; } = 0;

        /// <summary>八度（0-8）</summary>
        public int Octave { get; set; } = 4;

        /// <summary>持续时间（相对于Tempo的倍数）</summary>
        public float Duration { get; set; } = 1.0f;

        /// <summary>获取音符名称</summary>
        public string GetNoteName()
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            return $"{noteNames[Pitch]}{Octave}";
        }

        /// <summary>获取游戏内音效ID（基于星露谷的音效系统）</summary>
        public int GetGamePitch()
        {
            int basePitch = 1200;
            int octaveOffset = (Octave - 4) * 1200;
            int semitoneOffset = Pitch * 100;
            return basePitch + octaveOffset + semitoneOffset;
        }

        /// <summary>复制音符</summary>
        public MusicNote Clone()
        {
            return new MusicNote
            {
                Pitch = this.Pitch,
                Octave = this.Octave,
                Duration = this.Duration
            };
        }
    }
}
