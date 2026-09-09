using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BalartroLike.Battle
{
    public static class CsvBattleConfigLoader
    {
        public static void LoadFromDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Config directory is empty.", nameof(directory));
            }

            Dictionary<string, string> files = new Dictionary<string, string>();
            files.Add("trigrams.csv", File.ReadAllText(Path.Combine(directory, "trigrams.csv"), Encoding.UTF8));
            files.Add("hexagrams.csv", File.ReadAllText(Path.Combine(directory, "hexagrams.csv"), Encoding.UTF8));
            files.Add("weapons.csv", File.ReadAllText(Path.Combine(directory, "weapons.csv"), Encoding.UTF8));
            files.Add("enemies.csv", File.ReadAllText(Path.Combine(directory, "enemies.csv"), Encoding.UTF8));
            files.Add("deck.csv", File.ReadAllText(Path.Combine(directory, "deck.csv"), Encoding.UTF8));
            files.Add("battle_default.csv", File.ReadAllText(Path.Combine(directory, "battle_default.csv"), Encoding.UTF8));
            Load(files);
        }

        public static void Load(IReadOnlyDictionary<string, string> files)
        {
            CsvTable trigramTable = CsvTable.Parse(GetFile(files, "trigrams.csv"));
            CsvTable hexagramTable = CsvTable.Parse(GetFile(files, "hexagrams.csv"));
            CsvTable weaponTable = CsvTable.Parse(GetFile(files, "weapons.csv"));
            CsvTable enemyTable = CsvTable.Parse(GetFile(files, "enemies.csv"));
            CsvTable deckTable = CsvTable.Parse(GetFile(files, "deck.csv"));
            CsvTable defaultTable = CsvTable.Parse(GetFile(files, "battle_default.csv"));

            TrigramCatalog.Load(ParseTrigrams(trigramTable));
            HexagramCatalog.Load(ParseHexagrams(hexagramTable));
            BattleConfigDatabase.Load(
                ParseDefault(defaultTable),
                ParseWeapons(weaponTable),
                ParseEnemies(enemyTable),
                ParseDeck(deckTable));
        }

        private static string GetFile(IReadOnlyDictionary<string, string> files, string name)
        {
            if (files.TryGetValue(name, out string content))
            {
                return content;
            }

            throw new ArgumentException("Missing config file: " + name);
        }

        private static List<TrigramDefinition> ParseTrigrams(CsvTable table)
        {
            List<TrigramDefinition> definitions = new List<TrigramDefinition>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string[] row = table.Rows[i];
                definitions.Add(new TrigramDefinition(
                    ParseEnum<TrigramId>(table.Get(row, "id")),
                    table.Get(row, "displayName"),
                    table.Get(row, "symbol"),
                    ParseEnum<ElementType>(table.Get(row, "element")),
                    ParseEnum<YinYangType>(table.Get(row, "yinYang")),
                    ParseInt(table.Get(row, "baseQi")),
                    ParseInt(table.Get(row, "innerDamageMultiplier")),
                    ParseInt(table.Get(row, "innerHitCount")),
                    ParseEffects(table.Get(row, "innerEffects")),
                    ParseInt(table.Get(row, "outerDamageMultiplier")),
                    ParseBool(table.Get(row, "outerGuaranteedCritical")),
                    ParseEffects(table.Get(row, "outerEffects"))));
            }

            return definitions;
        }

        private static List<HexagramDefinition> ParseHexagrams(CsvTable table)
        {
            List<HexagramDefinition> definitions = new List<HexagramDefinition>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string[] row = table.Rows[i];
                definitions.Add(new HexagramDefinition(
                    table.Get(row, "id"),
                    table.Get(row, "displayName"),
                    ParseEnum<TrigramId>(table.Get(row, "outer")),
                    ParseEnum<TrigramId>(table.Get(row, "inner")),
                    ParseNullableInt(table.Get(row, "damageMultiplierOverride")),
                    ParseNullableBool(table.Get(row, "guaranteedCriticalOverride")),
                    ParseEffects(table.Get(row, "additionalEffects"))));
            }

            return definitions;
        }

        private static BattleDefaultDefinition ParseDefault(CsvTable table)
        {
            return new BattleDefaultDefinition(
                ParseInt(table.GetValue("playerMaxHp")),
                ParseInt(table.GetValue("playerMaxEnergy")),
                ParseInt(table.GetValue("energyPerTurn")),
                ParseInt(table.GetValue("discardLimit")),
                ParseInt(table.GetValue("handSize")),
                table.GetValue("defaultWeaponId"),
                table.GetValue("defaultEnemyId"),
                ParseInt(table.GetValue("defaultSeed")));
        }

        private static List<WeaponDefinition> ParseWeapons(CsvTable table)
        {
            List<WeaponDefinition> definitions = new List<WeaponDefinition>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string[] row = table.Rows[i];
                string affinity = table.Get(row, "elementAffinity");
                definitions.Add(new WeaponDefinition(
                    table.Get(row, "id"),
                    table.Get(row, "displayName"),
                    ParseInt(table.Get(row, "basePower")),
                    string.IsNullOrWhiteSpace(affinity) ? (ElementType?)null : ParseEnum<ElementType>(affinity),
                    ParseInt(table.Get(row, "maxEnchantSlots"))));
            }

            return definitions;
        }

        private static List<EnemyDefinition> ParseEnemies(CsvTable table)
        {
            List<EnemyDefinition> definitions = new List<EnemyDefinition>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string[] row = table.Rows[i];
                definitions.Add(new EnemyDefinition(
                    table.Get(row, "id"),
                    table.Get(row, "displayName"),
                    ParseEnum<ElementType>(table.Get(row, "element")),
                    ParseInt(table.Get(row, "maxHp")),
                    ParseInt(table.Get(row, "basePower"))));
            }

            return definitions;
        }

        private static List<DeckEntryDefinition> ParseDeck(CsvTable table)
        {
            List<DeckEntryDefinition> definitions = new List<DeckEntryDefinition>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string[] row = table.Rows[i];
                definitions.Add(new DeckEntryDefinition(
                    ParseEnum<TrigramId>(table.Get(row, "trigram")),
                    ParseInt(table.Get(row, "rank")),
                    ParseInt(table.Get(row, "qi")),
                    ParseInt(table.Get(row, "count"))));
            }

            return definitions;
        }

        private static EffectOperation[] ParseEffects(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new EffectOperation[0];
            }

            string[] parts = value.Split(';');
            List<EffectOperation> effects = new List<EffectOperation>();
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i]))
                {
                    continue;
                }

                string[] fields = parts[i].Split('|');
                if (fields.Length < 7)
                {
                    throw new FormatException("Invalid effect config: " + parts[i]);
                }

                effects.Add(new EffectOperation(
                    ParseEnum<EffectType>(fields[0]),
                    ParseEnum<EffectTarget>(fields[1]),
                    ParseEnum<ValueType>(fields[2]),
                    ParseInt(fields[3]),
                    ParseEnum<StatusId>(fields[4]),
                    ParseInt(fields[5]),
                    ParseInt(fields[6])));
            }

            return effects.ToArray();
        }

        private static int ParseInt(string value)
        {
            return int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static bool ParseBool(string value)
        {
            return bool.Parse(value);
        }

        private static int? ParseNullableInt(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (int?)null : ParseInt(value);
        }

        private static bool? ParseNullableBool(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (bool?)null : ParseBool(value);
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        private sealed class CsvTable
        {
            public string[] Headers { get; }
            public List<string[]> Rows { get; }

            private CsvTable(string[] headers, List<string[]> rows)
            {
                Headers = headers;
                Rows = rows;
            }

            public static CsvTable Parse(string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new ArgumentException("CSV content is empty.");
                }

                text = text.TrimStart('\uFEFF');
                List<string[]> parsedRows = ParseRows(text);
                if (parsedRows.Count == 0)
                {
                    throw new ArgumentException("CSV content has no rows.");
                }

                string[] headers = parsedRows[0];
                for (int i = 0; i < headers.Length; i++)
                {
                    headers[i] = headers[i].Trim();
                }

                List<string[]> rows = new List<string[]>();
                for (int i = 1; i < parsedRows.Count; i++)
                {
                    rows.Add(parsedRows[i]);
                }

                return new CsvTable(headers, rows);
            }

            public string Get(string[] row, string column)
            {
                int index = Array.IndexOf(Headers, column);
                if (index < 0 || index >= row.Length)
                {
                    return string.Empty;
                }

                return row[index].Trim();
            }

            public string GetValue(string key)
            {
                for (int i = 0; i < Rows.Count; i++)
                {
                    if (Get(Rows[i], "key") == key)
                    {
                        return Get(Rows[i], "value");
                    }
                }

                throw new ArgumentException("Missing config key: " + key);
            }

            private static List<string[]> ParseRows(string text)
            {
                List<string[]> rows = new List<string[]>();
                List<string> row = new List<string>();
                StringBuilder field = new StringBuilder();
                bool inQuotes = false;

                for (int i = 0; i < text.Length; i++)
                {
                    char character = text[i];
                    if (inQuotes)
                    {
                        if (character == '"')
                        {
                            if (i + 1 < text.Length && text[i + 1] == '"')
                            {
                                field.Append('"');
                                i++;
                            }
                            else
                            {
                                inQuotes = false;
                            }
                        }
                        else
                        {
                            field.Append(character);
                        }
                    }
                    else if (character == '"')
                    {
                        inQuotes = true;
                    }
                    else if (character == ',')
                    {
                        row.Add(field.ToString());
                        field.Length = 0;
                    }
                    else if (character == '\r' || character == '\n')
                    {
                        if (character == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        {
                            i++;
                        }

                        AddRow(rows, row, field);
                    }
                    else
                    {
                        field.Append(character);
                    }
                }

                if (field.Length > 0 || row.Count > 0)
                {
                    AddRow(rows, row, field);
                }

                return rows;
            }

            private static void AddRow(List<string[]> rows, List<string> row, StringBuilder field)
            {
                row.Add(field.ToString());
                field.Length = 0;
                bool hasValue = false;
                for (int i = 0; i < row.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(row[i]))
                    {
                        hasValue = true;
                        break;
                    }
                }

                if (hasValue)
                {
                    rows.Add(row.ToArray());
                }

                row.Clear();
            }
        }
    }
}