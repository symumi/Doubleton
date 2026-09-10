using System.Collections.Generic;
using BalartroLike.Battle;
using UnityEngine;

namespace BalartroLike.Unity
{
    // UI 素材统一入口：把战斗枚举映射到 Assets/Resources/Texture/UI 下的文件名，并缓存已加载的 Sprite。
    // 文件名规则见 docs/design/ui_source/ui_manifest.md。
    public static class UISpriteCatalog
    {
        public const string Root = "Texture/UI/";

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Load(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            if (Cache.TryGetValue(relativePath, out Sprite cached))
            {
                return cached;
            }

            Sprite sprite = Resources.Load<Sprite>(Root + relativePath);
            if (sprite == null)
            {
                Debug.LogWarning("UI sprite not found: " + Root + relativePath);
            }

            Cache[relativePath] = sprite;
            return sprite;
        }

        public static Sprite Trigram(TrigramId id)
        {
            return Load("Hexagram/bagua_" + TrigramKey(id));
        }

        public static Sprite Element(ElementType element)
        {
            return Load("Icon/element_" + ElementKey(element));
        }

        public static Sprite Status(StatusId id)
        {
            return id == StatusId.None ? null : Load("Icon/status_" + StatusKey(id));
        }

        public static Sprite Intent(EnemyIntentType type)
        {
            return Load("Icon/intent_" + IntentKey(type));
        }

        public static Sprite RealmSeal(int level)
        {
            return Load("Icon/realm_seal_" + Mathf.Clamp(level, 1, 3));
        }

        public static Sprite Resource(string name)
        {
            return Load("Icon/icon_" + name);
        }

        public static Sprite Panel(string fileName)
        {
            return Load("Common/" + fileName);
        }

        public static Sprite Button(string fileName)
        {
            return Load("Button/" + fileName);
        }

        public static Sprite Frame(string fileName)
        {
            return Load("Frame/" + fileName);
        }

        public static Sprite CardFrameForRank(int rank)
        {
            return Load("Frame/card_frame_rank" + Mathf.Clamp(rank, 1, 3) + "_9s");
        }

        public static Sprite Background(string fileName)
        {
            return Load("Background/" + fileName);
        }

        private static string TrigramKey(TrigramId id)
        {
            switch (id)
            {
                case TrigramId.Qian:
                    return "qian";
                case TrigramId.Dui:
                    return "dui";
                case TrigramId.Li:
                    return "li";
                case TrigramId.Zhen:
                    return "zhen";
                case TrigramId.Xun:
                    return "xun";
                case TrigramId.Kan:
                    return "kan";
                case TrigramId.Gen:
                    return "gen";
                default:
                    return "kun";
            }
        }

        private static string ElementKey(ElementType element)
        {
            switch (element)
            {
                case ElementType.Metal:
                    return "metal";
                case ElementType.Wood:
                    return "wood";
                case ElementType.Water:
                    return "water";
                case ElementType.Fire:
                    return "fire";
                default:
                    return "earth";
            }
        }

        private static string StatusKey(StatusId id)
        {
            switch (id)
            {
                case StatusId.Burn:
                    return "burn";
                case StatusId.Vulnerable:
                    return "vulnerable";
                case StatusId.Weak:
                    return "weak";
                case StatusId.Shield:
                    return "shield";
                case StatusId.ArmorBreak:
                    return "armor_break";
                default:
                    return "chill";
            }
        }

        private static string IntentKey(EnemyIntentType type)
        {
            switch (type)
            {
                case EnemyIntentType.Defend:
                    return "defend";
                case EnemyIntentType.Debuff:
                    return "debuff";
                default:
                    return "attack";
            }
        }
    }
}
