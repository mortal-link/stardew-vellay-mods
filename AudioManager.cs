using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;

namespace PianoBlock
{
    public class AudioManager
    {
        private static AudioManager? _instance;
        public static AudioManager Instance => _instance ??= new AudioManager();

        // 旧版单层采样 (piano_*.wav)
        private readonly Dictionary<int, SoundEffect> _pianoSamples = new();

        // 新版多力度层采样 (piano-velocity/*.wav)
        // Key: (absolutePitch, velocityLayer 1-6)
        private readonly Dictionary<(int pitch, int velocity), SoundEffect> _velocitySamples = new();

        // 采样音高列表 (用于快速查找最近采样)
        private readonly List<int> _availablePitches = new();

        // 力度层数量
        private const int VelocityLayers = 6;

        // 是否启用力度层
        private bool _useVelocityLayers = false;

        private IMonitor? _monitor;
        private IModHelper? _helper;

        // 活跃的音符实例，用于控制延音
        private readonly List<ActiveNote> _activeNotes = new();

        private class ActiveNote
        {
            public SoundEffectInstance Instance { get; set; } = null!;
            public long StopTime { get; set; }
        }

        public void Initialize(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            LoadVelocitySamples(); // 先尝试加载力度层采样
            if (!_useVelocityLayers)
            {
                LoadSamples(); // 回退到旧版单层采样
            }
        }

        /// <summary>每帧更新，检查需要停止的音符</summary>
        public void Update()
        {
            if (_activeNotes.Count == 0) return;

            long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var note = _activeNotes[i];
                if (currentTime >= note.StopTime)
                {
                    try
                    {
                        note.Instance.Stop();
                        note.Instance.Dispose();
                    }
                    catch { }
                    _activeNotes.RemoveAt(i);
                }
            }
        }

        private void LoadSamples()
        {
            try
            {
                string audioDir = Path.Combine(_helper!.DirectoryPath, "assets", "audio");
                _monitor?.Log($"Looking for audio in: {audioDir}", LogLevel.Info);

                if (!Directory.Exists(audioDir))
                {
                    _monitor?.Log($"Audio directory NOT found at: {audioDir}", LogLevel.Error);
                    return;
                }

                string[] files = Directory.GetFiles(audioDir, "piano_*.wav");
                _monitor?.Log($"Found {files.Length} piano files in directory.", LogLevel.Info);

                foreach (string file in files)
                {
                    try
                    {
                        using var stream = File.OpenRead(file);
                        var sound = SoundEffect.FromStream(stream);

                        string filename = Path.GetFileNameWithoutExtension(file);
                        string notePart = filename.Replace("piano_", "");

                        if (TryParseNoteIndex(notePart, out int absolutePitch))
                        {
                            _pianoSamples[absolutePitch] = sound;
                            _monitor?.Log($"Loaded piano sample: {filename} (Pitch: {absolutePitch})", LogLevel.Trace);
                        }
                    }
                    catch (Exception ex)
                    {
                        _monitor?.Log($"Failed to load audio file {file}: {ex.Message}", LogLevel.Error);
                    }
                }

                _monitor?.Log($"Total piano samples loaded: {_pianoSamples.Count}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor?.Log($"Error loading audio samples: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>加载力度层采样 (piano-velocity 目录)</summary>
        private void LoadVelocitySamples()
        {
            try
            {
                string velocityDir = Path.Combine(_helper!.DirectoryPath, "assets", "piano-velocity");
                _monitor?.Log($"Looking for velocity samples in: {velocityDir}", LogLevel.Info);

                if (!Directory.Exists(velocityDir))
                {
                    _monitor?.Log($"Velocity samples directory not found, will use legacy samples.", LogLevel.Debug);
                    return;
                }

                string[] files = Directory.GetFiles(velocityDir, "*_v*.wav");
                _monitor?.Log($"Found {files.Length} velocity sample files.", LogLevel.Info);

                if (files.Length < 30) // 至少需要一层的采样
                {
                    _monitor?.Log($"Not enough velocity samples ({files.Length}), need at least 30.", LogLevel.Warn);
                    return;
                }

                foreach (string file in files)
                {
                    try
                    {
                        string filename = Path.GetFileNameWithoutExtension(file);
                        // 格式: A0_v1, C4_v3, D#5_v6 等
                        int vIndex = filename.LastIndexOf("_v");
                        if (vIndex == -1) continue;

                        string notePart = filename.Substring(0, vIndex);
                        string velPart = filename.Substring(vIndex + 2);

                        if (!int.TryParse(velPart, out int velocityLayer)) continue;
                        if (velocityLayer < 1 || velocityLayer > VelocityLayers) continue;

                        if (!TryParseNoteIndex(notePart, out int absolutePitch)) continue;

                        using var stream = File.OpenRead(file);
                        var sound = SoundEffect.FromStream(stream);

                        _velocitySamples[(absolutePitch, velocityLayer)] = sound;

                        // 记录可用音高
                        if (!_availablePitches.Contains(absolutePitch))
                        {
                            _availablePitches.Add(absolutePitch);
                        }

                        _monitor?.Log($"Loaded velocity sample: {filename} (Pitch: {absolutePitch}, Vel: {velocityLayer})", LogLevel.Trace);
                    }
                    catch (Exception ex)
                    {
                        _monitor?.Log($"Failed to load velocity sample {file}: {ex.Message}", LogLevel.Warn);
                    }
                }

                _availablePitches.Sort();

                if (_velocitySamples.Count >= 30)
                {
                    _useVelocityLayers = true;
                    _monitor?.Log($"Velocity samples enabled: {_velocitySamples.Count} samples, {_availablePitches.Count} pitches", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"Error loading velocity samples: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>根据 MIDI velocity (0-127) 获取力度层 (1-6)</summary>
        private int GetVelocityLayer(int midiVelocity)
        {
            // 6 层力度映射
            // v1: 0-21 (pp), v2: 22-42 (p), v3: 43-63 (mp)
            // v4: 64-84 (mf), v5: 85-105 (f), v6: 106-127 (ff)
            if (midiVelocity <= 21) return 1;
            if (midiVelocity <= 42) return 2;
            if (midiVelocity <= 63) return 3;
            if (midiVelocity <= 84) return 4;
            if (midiVelocity <= 105) return 5;
            return 6;
        }

        /// <summary>找到最接近的采样音高</summary>
        private int FindNearestSamplePitch(int targetPitch)
        {
            if (_availablePitches.Count == 0) return -1;

            int bestPitch = _availablePitches[0];
            int minDist = Math.Abs(targetPitch - bestPitch);

            foreach (int pitch in _availablePitches)
            {
                int dist = Math.Abs(targetPitch - pitch);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestPitch = pitch;
                }
            }

            return bestPitch;
        }

        private bool TryParseNoteIndex(string noteStr, out int absolutePitch)
        {
            absolutePitch = -1;
            if (string.IsNullOrEmpty(noteStr)) return false;

            int digitIndex = -1;
            for (int i = 0; i < noteStr.Length; i++)
            {
                if (char.IsDigit(noteStr[i]))
                {
                    digitIndex = i;
                    break;
                }
            }

            if (digitIndex == -1) return false;

            string noteName = noteStr.Substring(0, digitIndex);
            string octaveStr = noteStr.Substring(digitIndex);

            if (!int.TryParse(octaveStr, out int octave)) return false;

            int pitchInOctave = GetPitchFromNoteName(noteName);
            if (pitchInOctave == -1) return false;

            absolutePitch = octave * 12 + pitchInOctave;
            return true;
        }

        private int GetPitchFromNoteName(string name)
        {
            return name.ToUpper() switch
            {
                "C" => 0,
                "C#" => 1, "DB" => 1,
                "D" => 2,
                "D#" => 3, "EB" => 3,
                "E" => 4,
                "F" => 5,
                "F#" => 6, "GB" => 6,
                "G" => 7,
                "G#" => 8, "AB" => 8,
                "A" => 9,
                "A#" => 10, "BB" => 10,
                "B" => 11,
                _ => -1
            };
        }

        /// <summary>播放钢琴音符</summary>
        /// <param name="pitch">音高 0-11</param>
        /// <param name="octave">八度 1-7</param>
        /// <param name="durationMs">延音时长（毫秒），0表示自然衰减</param>
        /// <param name="velocity">力度 0-127，默认 100</param>
        public void PlayPianoNote(int pitch, int octave, int durationMs = 0, int velocity = 100)
        {
            int targetAbsolutePitch = octave * 12 + pitch;

            // 优先使用力度层采样
            if (_useVelocityLayers)
            {
                PlayWithVelocity(targetAbsolutePitch, velocity, durationMs);
                return;
            }

            // 回退到旧版单层采样
            if (_pianoSamples.Count == 0)
            {
                try {
                    int gamePitch = 1200 + (octave - 4) * 1200 + pitch * 100;
                    Game1.playSound("flute", gamePitch);
                } catch { Game1.playSound("flute"); }
                return;
            }

            // 找到最接近的采样
            int bestSamplePitch = -1;
            int minDistance = int.MaxValue;

            foreach (var samplePitch in _pianoSamples.Keys)
            {
                int dist = Math.Abs(targetAbsolutePitch - samplePitch);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestSamplePitch = samplePitch;
                }
            }

            if (bestSamplePitch != -1 && _pianoSamples.TryGetValue(bestSamplePitch, out var effect))
            {
                int semitoneDiff = targetAbsolutePitch - bestSamplePitch;
                float pitchParam = Math.Clamp(semitoneDiff / 12.0f, -1.0f, 1.0f);

                try
                {
                    var instance = effect.CreateInstance();
                    instance.Pitch = pitchParam;
                    instance.Volume = 1.0f;
                    instance.Play();

                    // 如果指定了延音时长，记录以便后续停止
                    if (durationMs > 0)
                    {
                        long stopTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + durationMs;
                        _activeNotes.Add(new ActiveNote
                        {
                            Instance = instance,
                            StopTime = stopTime
                        });
                    }
                    // 如果 durationMs == 0，让声音自然衰减（不管理实例）
                }
                catch (Exception ex)
                {
                    _monitor?.Log($"Failed to play note {targetAbsolutePitch}: {ex.Message}", LogLevel.Warn);
                }
            }
        }

        /// <summary>使用力度层采样播放音符</summary>
        private void PlayWithVelocity(int targetAbsolutePitch, int velocity, int durationMs)
        {
            int velocityLayer = GetVelocityLayer(velocity);
            int samplePitch = FindNearestSamplePitch(targetAbsolutePitch);

            if (samplePitch == -1)
            {
                _monitor?.Log($"No sample found for pitch {targetAbsolutePitch}", LogLevel.Warn);
                return;
            }

            var key = (samplePitch, velocityLayer);
            if (!_velocitySamples.TryGetValue(key, out var effect))
            {
                // 如果该力度层没有采样，尝试相邻力度层
                for (int delta = 1; delta < VelocityLayers; delta++)
                {
                    if (_velocitySamples.TryGetValue((samplePitch, velocityLayer + delta), out effect)) break;
                    if (_velocitySamples.TryGetValue((samplePitch, velocityLayer - delta), out effect)) break;
                }
            }

            if (effect == null)
            {
                _monitor?.Log($"No velocity sample for pitch {samplePitch}, layer {velocityLayer}", LogLevel.Warn);
                return;
            }

            // 计算音高偏移
            int semitoneDiff = targetAbsolutePitch - samplePitch;
            float pitchParam = Math.Clamp(semitoneDiff / 12.0f, -1.0f, 1.0f);

            try
            {
                var instance = effect.CreateInstance();
                instance.Pitch = pitchParam;
                instance.Volume = 1.0f;
                instance.Play();

                if (durationMs > 0)
                {
                    long stopTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + durationMs;
                    _activeNotes.Add(new ActiveNote
                    {
                        Instance = instance,
                        StopTime = stopTime
                    });
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"Failed to play velocity note: {ex.Message}", LogLevel.Warn);
            }
        }
    }
}
