using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace BalartroLike.Unity
{
    // 单条 UI 动效参数。字段含义与 battle_feedback_animation.csv 保持一致，便于统一调整节奏。
    public sealed class UIAnimationEntry
    {
        public string Key { get; private set; }
        public float Duration { get; private set; }
        public UITweenEasing Easing { get; private set; }
        public float OffsetY { get; private set; }
        public float StartScale { get; private set; }
        public float EndScale { get; private set; }
        public float ShakeDistance { get; private set; }
        public float ShakeFrequency { get; private set; }
        public float FadeStart { get; private set; }
        public float PunchScale { get; private set; }
        public float Delay { get; private set; }

        public static UIAnimationEntry CreateDefault(string key)
        {
            return new UIAnimationEntry
            {
                Key = key,
                Duration = 0.18f,
                Easing = UITweenEasing.OutCubic,
                OffsetY = 0f,
                StartScale = 1f,
                EndScale = 1f,
                ShakeDistance = 0f,
                ShakeFrequency = 0f,
                FadeStart = 1f,
                PunchScale = 1f,
                Delay = 0f,
            };
        }

        public bool HasFade => FadeStart < 1f;

        public bool HasPunch => !Mathf.Approximately(PunchScale, StartScale);

        internal void Apply(string[] cells, Dictionary<string, int> columns)
        {
            Duration = ReadFloat(cells, columns, "duration", Duration);
            OffsetY = ReadFloat(cells, columns, "offsetY", OffsetY);
            StartScale = ReadFloat(cells, columns, "startScale", StartScale);
            EndScale = ReadFloat(cells, columns, "endScale", EndScale);
            ShakeDistance = ReadFloat(cells, columns, "shakeDistance", ShakeDistance);
            ShakeFrequency = ReadFloat(cells, columns, "shakeFrequency", ShakeFrequency);
            FadeStart = ReadFloat(cells, columns, "fadeStart", FadeStart);
            PunchScale = ReadFloat(cells, columns, "punchScale", PunchScale);
            Delay = ReadFloat(cells, columns, "delay", Delay);
            Easing = ReadEasing(cells, columns, "easing", Easing);
        }

        private static float ReadFloat(string[] cells, Dictionary<string, int> columns, string column, float fallback)
        {
            string value = ReadCell(cells, columns, column);
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : fallback;
        }

        private static UITweenEasing ReadEasing(string[] cells, Dictionary<string, int> columns, string column, UITweenEasing fallback)
        {
            string value = ReadCell(cells, columns, column);
            return Enum.TryParse(value, true, out UITweenEasing parsed) ? parsed : fallback;
        }

        private static string ReadCell(string[] cells, Dictionary<string, int> columns, string column)
        {
            if (!columns.TryGetValue(column, out int index) || index >= cells.Length)
            {
                return string.Empty;
            }

            return cells[index].Trim();
        }
    }

    // 读取 Assets/Resources/Config/UI/ui_animation.csv，缺失的 key 回退到默认参数。
    public sealed class UIAnimationConfig
    {
        public const string ResourcePath = "Config/UI/ui_animation";

        private readonly Dictionary<string, UIAnimationEntry> _entries = new Dictionary<string, UIAnimationEntry>(StringComparer.Ordinal);
        private readonly UIAnimationEntry _fallback = UIAnimationEntry.CreateDefault(string.Empty);

        public int Count => _entries.Count;

        public static UIAnimationConfig Load()
        {
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            return asset == null ? new UIAnimationConfig() : Parse(asset.text);
        }

        public UIAnimationEntry Get(string key)
        {
            return TryGet(key, out UIAnimationEntry entry) ? entry : _fallback;
        }

        public bool TryGet(string key, out UIAnimationEntry entry)
        {
            if (string.IsNullOrEmpty(key))
            {
                entry = _fallback;
                return false;
            }

            return _entries.TryGetValue(key, out entry);
        }

        public IEnumerable<string> Keys => _entries.Keys;

        public static UIAnimationConfig Parse(string csv)
        {
            UIAnimationConfig config = new UIAnimationConfig();
            if (string.IsNullOrWhiteSpace(csv))
            {
                return config;
            }

            string[] lines = csv.TrimStart('\uFEFF').Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            Dictionary<string, int> columns = null;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] cells = line.Split(',');
                if (columns == null)
                {
                    columns = BuildColumns(cells);
                    continue;
                }

                string key = cells.Length > 0 ? cells[0].Trim() : string.Empty;
                if (key.Length == 0)
                {
                    continue;
                }

                UIAnimationEntry entry = UIAnimationEntry.CreateDefault(key);
                entry.Apply(cells, columns);
                config._entries[key] = entry;
            }

            return config;
        }

        private static Dictionary<string, int> BuildColumns(string[] headers)
        {
            Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < headers.Length; i++)
            {
                string name = headers[i].Trim();
                if (name.Length > 0 && !columns.ContainsKey(name))
                {
                    columns.Add(name, i);
                }
            }

            return columns;
        }
    }
}
