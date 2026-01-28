using System.Collections.Generic;

namespace PianoBlock
{
    /// <summary>钢琴块的配置数据</summary>
    public class PianoBlockData
    {
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
    }

    /// <summary>单个音符</summary>
    public class MusicNote
    {
        /// <summary>音高（0-11对应C到B）</summary>
        public int Pitch { get; set; } = 0; // 0=C, 1=C#, 2=D, 3=D#, 4=E, 5=F, 6=F#, 7=G, 8=G#, 9=A, 10=A#, 11=B

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
            // 将音符映射到游戏的音高值
            // 基础音高 + 八度偏移 + 半音偏移
            int basePitch = 1200; // 基础音高
            int octaveOffset = (Octave - 4) * 1200; // 每个八度12个半音
            int semitoneOffset = Pitch * 100; // 每个半音

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
