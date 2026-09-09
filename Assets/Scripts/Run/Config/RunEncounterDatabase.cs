using System;
using System.Collections.Generic;

namespace BalartroLike.Run
{
    public static class RunEncounterDatabase
    {
        private static readonly List<RunShopOfferDefinition> ShopOfferDefinitions = new List<RunShopOfferDefinition>();
        private static readonly List<RunEventOptionDefinition> EventOptionDefinitions = new List<RunEventOptionDefinition>();

        public static bool IsLoaded
        {
            get { return ShopOfferDefinitions.Count > 0 && EventOptionDefinitions.Count > 0; }
        }

        public static void Load(IEnumerable<RunShopOfferDefinition> shopOffers, IEnumerable<RunEventOptionDefinition> eventOptions)
        {
            ShopOfferDefinitions.Clear();
            EventOptionDefinitions.Clear();
            HashSet<string> shopIds = new HashSet<string>();
            HashSet<string> eventOptionIds = new HashSet<string>();

            foreach (RunShopOfferDefinition offer in shopOffers)
            {
                if (!shopIds.Add(offer.Id))
                {
                    throw new InvalidOperationException("Duplicate shop offer id: " + offer.Id);
                }

                ShopOfferDefinitions.Add(offer);
            }

            foreach (RunEventOptionDefinition option in eventOptions)
            {
                if (!eventOptionIds.Add(option.EventId + ":" + option.OptionId))
                {
                    throw new InvalidOperationException("Duplicate event option id: " + option.EventId + ":" + option.OptionId);
                }

                EventOptionDefinitions.Add(option);
            }

            if (!IsLoaded)
            {
                throw new InvalidOperationException("Run encounter config is incomplete.");
            }
        }

        public static IReadOnlyList<RunShopOfferDefinition> GetShopOffers(string shopId)
        {
            List<RunShopOfferDefinition> result = new List<RunShopOfferDefinition>();
            for (int i = 0; i < ShopOfferDefinitions.Count; i++)
            {
                if (ShopOfferDefinitions[i].ShopId == shopId)
                {
                    result.Add(ShopOfferDefinitions[i]);
                }
            }

            return result;
        }

        public static IReadOnlyList<RunEventOptionDefinition> GetEventOptions(string eventId)
        {
            List<RunEventOptionDefinition> result = new List<RunEventOptionDefinition>();
            for (int i = 0; i < EventOptionDefinitions.Count; i++)
            {
                if (EventOptionDefinitions[i].EventId == eventId)
                {
                    result.Add(EventOptionDefinitions[i]);
                }
            }

            return result;
        }

        public static bool TryGetShopOffer(string offerId, out RunShopOfferDefinition offer)
        {
            for (int i = 0; i < ShopOfferDefinitions.Count; i++)
            {
                if (ShopOfferDefinitions[i].Id == offerId)
                {
                    offer = ShopOfferDefinitions[i];
                    return true;
                }
            }

            offer = null;
            return false;
        }

        public static bool TryGetEventOption(string eventId, string optionId, out RunEventOptionDefinition option)
        {
            for (int i = 0; i < EventOptionDefinitions.Count; i++)
            {
                RunEventOptionDefinition candidate = EventOptionDefinitions[i];
                if (candidate.EventId == eventId && candidate.OptionId == optionId)
                {
                    option = candidate;
                    return true;
                }
            }

            option = null;
            return false;
        }
    }
}
