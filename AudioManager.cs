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

        private readonly Dictionary<int, SoundEffect> _pianoSamples = new();
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
            LoadSamples();
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
        public void PlayPianoNote(int pitch, int octave, int durationMs = 0)
        {
            int targetAbsolutePitch = octave * 12 + pitch;

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
    }
}
