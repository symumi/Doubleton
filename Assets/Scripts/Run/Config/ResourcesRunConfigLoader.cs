using System;
using UnityEngine;

namespace BalartroLike.Run
{
    public static class ResourcesRunConfigLoader
    {
        private const string NodeConfigPath = "Config/Run/run_nodes";
        private const string EncounterConfigPath = "Config/Run/run_encounters";
        private const string RealmConfigPath = "Config/Run/run_realms";
        private const string TribulationConfigPath = "Config/Run/run_tribulations";
        private const string ShopOfferConfigPath = "Config/Run/run_shop_offers";
        private const string EventOptionConfigPath = "Config/Run/run_event_options";

        public static void LoadIfNeeded()
        {
            if (RunConfigDatabase.IsLoaded && RunEncounterDatabase.IsLoaded)
            {
                return;
            }

            TextAsset nodeAsset = Resources.Load<TextAsset>(NodeConfigPath);
            TextAsset encounterAsset = Resources.Load<TextAsset>(EncounterConfigPath);
            TextAsset realmAsset = Resources.Load<TextAsset>(RealmConfigPath);
            TextAsset tribulationAsset = Resources.Load<TextAsset>(TribulationConfigPath);
            TextAsset shopOfferAsset = Resources.Load<TextAsset>(ShopOfferConfigPath);
            TextAsset eventOptionAsset = Resources.Load<TextAsset>(EventOptionConfigPath);
            if (nodeAsset == null || encounterAsset == null || realmAsset == null || tribulationAsset == null || shopOfferAsset == null || eventOptionAsset == null)
            {
                throw new InvalidOperationException("Missing run config resource.");
            }

            CsvRunConfigLoader.Load(nodeAsset.text, encounterAsset.text, realmAsset.text, tribulationAsset.text);
            CsvRunConfigLoader.LoadEncounters(shopOfferAsset.text, eventOptionAsset.text);
        }
    }
}