using System;
using System.Collections.Generic;
using UnityEngine;

namespace BalartroLike.Battle
{
    public static class ResourcesBattleConfigLoader
    {
        private const string ConfigRoot = "Config/Battle/";
        private static readonly string[] ConfigFiles =
        {
            "trigrams.csv",
            "hexagrams.csv",
            "weapons.csv",
            "enemies.csv",
            "deck.csv",
            "battle_default.csv",
        };

        public static void LoadIfNeeded()
        {
            if (BattleConfigDatabase.IsLoaded)
            {
                return;
            }

            Dictionary<string, string> files = new Dictionary<string, string>();
            for (int i = 0; i < ConfigFiles.Length; i++)
            {
                string fileName = ConfigFiles[i];
                string resourcePath = ConfigRoot + fileName.Substring(0, fileName.Length - 4);
                TextAsset asset = Resources.Load<TextAsset>(resourcePath);
                if (asset == null)
                {
                    throw new InvalidOperationException("Missing battle config resource: " + resourcePath);
                }

                files.Add(fileName, asset.text);
            }

            CsvBattleConfigLoader.Load(files);
        }
    }
}