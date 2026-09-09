using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BalartroLike.Run
{
    public static class CsvRunConfigLoader
    {
        public static void LoadFromDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Config directory is empty.", nameof(directory));
            }

            string nodeContent = File.ReadAllText(Path.Combine(directory, "run_nodes.csv"), Encoding.UTF8);
            string encounterContent = File.ReadAllText(Path.Combine(directory, "run_encounters.csv"), Encoding.UTF8);
            string realmContent = File.ReadAllText(Path.Combine(directory, "run_realms.csv"), Encoding.UTF8);
            string tribulationContent = File.ReadAllText(Path.Combine(directory, "run_tribulations.csv"), Encoding.UTF8);
            Load(nodeContent, encounterContent, realmContent, tribulationContent);
        }

        public static void Load(string nodeContent, string encounterContent, string realmContent)
        {
            Load(nodeContent, encounterContent, realmContent, null);
        }

        public static void Load(string nodeContent, string encounterContent, string realmContent, string tribulationContent)
        {
            if (string.IsNullOrWhiteSpace(nodeContent))
            {
                throw new ArgumentException("Run config is empty.", nameof(nodeContent));
            }

            Dictionary<string, EncounterRow> encounters = ParseEncounters(encounterContent);
            string[] lines = nodeContent.TrimStart('\uFEFF').Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                throw new FormatException("Run config has no data rows.");
            }

            string[] headers = lines[0].Split(',');
            List<RunNodeDefinition> nodes = new List<RunNodeDefinition>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] row = lines[i].Split(',');
                string id = Get(headers, row, "id");
                EncounterRow encounter;
                if (!encounters.TryGetValue(id, out encounter))
                {
                    encounter = EncounterRow.None;
                }

                nodes.Add(new RunNodeDefinition(
                    id,
                    Get(headers, row, "realmId"),
                    Get(headers, row, "displayName"),
                    ParseEnum<RunNodeType>(Get(headers, row, "type")),
                    ParseStringList(Get(headers, row, "enemyId")),
                    ParseInt(Get(headers, row, "rewardSpiritStones")),
                    encounter.Type,
                    encounter.Id));
            }

            RunConfigDatabase.Load(nodes, ParseRealms(realmContent), ParseTribulations(tribulationContent));
        }

        public static void LoadEncounters(string shopOfferContent, string eventOptionContent)
        {
            if (string.IsNullOrWhiteSpace(shopOfferContent) || string.IsNullOrWhiteSpace(eventOptionContent))
            {
                throw new ArgumentException("Run encounter config is empty.");
            }

            List<RunShopOfferDefinition> shopOffers = new List<RunShopOfferDefinition>();
            string[] shopLines = shopOfferContent.TrimStart('\uFEFF').Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] shopHeaders = shopLines[0].Split(',');
            for (int i = 1; i < shopLines.Length; i++)
            {
                string[] row = shopLines[i].Split(',');
                shopOffers.Add(new RunShopOfferDefinition(
                    Get(shopHeaders, row, "id"),
                    Get(shopHeaders, row, "shopId"),
                    Get(shopHeaders, row, "displayName"),
                    ParseEnum<RunEffectType>(Get(shopHeaders, row, "effectType")),
                    ParseInt(Get(shopHeaders, row, "effectValue")),
                    ParseInt(Get(shopHeaders, row, "price")),
                    Get(shopHeaders, row, "contentId"),
                    Get(shopHeaders, row, "description")));
            }

            List<RunEventOptionDefinition> eventOptions = new List<RunEventOptionDefinition>();
            string[] eventLines = eventOptionContent.TrimStart('\uFEFF').Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] eventHeaders = eventLines[0].Split(',');
            for (int i = 1; i < eventLines.Length; i++)
            {
                string[] row = eventLines[i].Split(',');
                eventOptions.Add(new RunEventOptionDefinition(
                    Get(eventHeaders, row, "eventId"),
                    Get(eventHeaders, row, "title"),
                    Get(eventHeaders, row, "description"),
                    Get(eventHeaders, row, "optionId"),
                    Get(eventHeaders, row, "optionText"),
                    ParseEnum<RunEffectType>(Get(eventHeaders, row, "effectType")),
                    ParseInt(Get(eventHeaders, row, "effectValue")),
                    Get(eventHeaders, row, "resultText")));
            }

            RunEncounterDatabase.Load(shopOffers, eventOptions);
        }

        private static List<HeavenTribulationDefinition> ParseTribulations(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string[] lines = content.TrimStart('\uFEFF').Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                throw new FormatException("Run tribulation config has no data rows.");
            }

            string[] headers = lines[0].Split(',');
            List<HeavenTribulationDefinition> tribulations = new List<HeavenTribulationDefinition>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] row = lines[i].Split(',');
                tribulations.Add(new HeavenTribulationDefinition(
                    ParseInt(Get(headers, row, "level")),
                    Get(headers, row, "displayName"),
                    ParseInt(Get(headers, row, "enemyHpPercent")),
                    ParseInt(Get(headers, row, "enemyPowerBonus")),
                    ParseInt(Get(headers, row, "shopPricePercent")),
                    Get(headers, row, "description")));
            }

            return tribulations;
        }

        private static List<RunRealmDefinition> ParseRealms(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Run realm config is empty.", nameof(content));
            }

            string[] lines = content.TrimStart('\uFEFF').Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                throw new FormatException("Run realm config has no data rows.");
            }

            string[] headers = lines[0].Split(',');
            List<RunRealmDefinition> realms = new List<RunRealmDefinition>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] row = lines[i].Split(',');
                realms.Add(new RunRealmDefinition(
                    Get(headers, row, "id"),
                    Get(headers, row, "displayName"),
                    Get(headers, row, "promotionTargetRealmId"),
                    ParseInt(Get(headers, row, "promotionRewardSpiritStones"))));
            }

            return realms;
        }

        private static Dictionary<string, EncounterRow> ParseEncounters(string content)
        {
            Dictionary<string, EncounterRow> encounters = new Dictionary<string, EncounterRow>();
            if (string.IsNullOrWhiteSpace(content))
            {
                return encounters;
            }

            string[] lines = content.TrimStart('\uFEFF').Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                return encounters;
            }

            string[] headers = lines[0].Split(',');
            for (int i = 1; i < lines.Length; i++)
            {
                string[] row = lines[i].Split(',');
                string nodeId = Get(headers, row, "nodeId");
                encounters.Add(nodeId, new EncounterRow(
                    ParseEnum<RunEncounterType>(Get(headers, row, "encounterType")),
                    Get(headers, row, "encounterId")));
            }

            return encounters;
        }

        private static string Get(string[] headers, string[] row, string column)
        {
            int index = Array.IndexOf(headers, column);
            if (index < 0 || index >= row.Length)
            {
                return string.Empty;
            }

            return row[index].Trim();
        }

        private static string[] ParseStringList(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new string[0]
                : value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static int ParseInt(string value)
        {
            return int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        private sealed class EncounterRow
        {
            public static readonly EncounterRow None = new EncounterRow(RunEncounterType.None, string.Empty);

            public RunEncounterType Type { get; }
            public string Id { get; }

            public EncounterRow(RunEncounterType type, string id)
            {
                Type = type;
                Id = id;
            }
        }
    }
}