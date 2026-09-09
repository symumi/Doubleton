using System;
using System.Globalization;
using UnityEngine;

namespace BalartroLike.Unity
{
    public sealed class BattleFeedbackAnimationConfig
    {
        public const string ResourcePath = "Config/UI/battle_feedback_animation";

        public float Duration { get; private set; }
        public float RiseDistance { get; private set; }
        public float StartScale { get; private set; }
        public float PunchScale { get; private set; }
        public float EndScale { get; private set; }
        public float ShakeDistance { get; private set; }
        public float ShakeFrequency { get; private set; }
        public float FadeStart { get; private set; }
        public float PunchTime { get; private set; }

        public static BattleFeedbackAnimationConfig Load()
        {
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            return asset == null ? CreateDefault() : Parse(asset.text);
        }

        public static BattleFeedbackAnimationConfig CreateDefault()
        {
            BattleFeedbackAnimationConfig config = new BattleFeedbackAnimationConfig();
            config.SetDefaults();
            return config;
        }

        public static BattleFeedbackAnimationConfig Parse(string csv)
        {
            BattleFeedbackAnimationConfig config = CreateDefault();
            if (string.IsNullOrWhiteSpace(csv))
            {
                return config;
            }

            string[] lines = csv.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("key,", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int separator = line.IndexOf(',');
                if (separator < 0)
                {
                    continue;
                }

                string key = line.Substring(0, separator).Trim();
                string value = line.Substring(separator + 1).Trim();
                config.Apply(key, value);
            }

            return config;
        }

        private void SetDefaults()
        {
            Duration = 0.55f;
            RiseDistance = 44f;
            StartScale = 0.82f;
            PunchScale = 1.16f;
            EndScale = 1f;
            ShakeDistance = 6f;
            ShakeFrequency = 3f;
            FadeStart = 0.55f;
            PunchTime = 0.18f;
        }

        private void Apply(string key, string value)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
            {
                return;
            }

            switch (key)
            {
                case "duration":
                    Duration = number;
                    break;
                case "riseDistance":
                    RiseDistance = number;
                    break;
                case "startScale":
                    StartScale = number;
                    break;
                case "punchScale":
                    PunchScale = number;
                    break;
                case "endScale":
                    EndScale = number;
                    break;
                case "shakeDistance":
                    ShakeDistance = number;
                    break;
                case "shakeFrequency":
                    ShakeFrequency = number;
                    break;
                case "fadeStart":
                    FadeStart = number;
                    break;
                case "punchTime":
                    PunchTime = number;
                    break;
            }
        }
    }
}