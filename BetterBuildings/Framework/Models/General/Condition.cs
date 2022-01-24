using BetterBuildings.Framework.Models.ContentPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBuildings.Framework.Models.General
{
    public class Condition
    {
        public enum Comparison
        {
            EqualTo,
            GreaterThan,
            LessThan,
            Contains
        }

        public enum Type
        {
            Unknown,
            InputItemNames,
            InputItemCount,
            OutputItemNames,
            OutputItemCount,
            HasFlag,
            HasReceivedMail,
            HasSeenEvent,
            IsPlayerOnWalkableTile
        }

        public Type Name { get; set; }
        public object Value { get; set; }
        public Comparison Operator { get; set; } = Comparison.EqualTo;
        public bool Inverse { get; set; }
        public bool Independent { get; set; }

        private object ParsedValue { get; set; }
        private object Cache { get; set; }

        internal bool IsValid(GenericBuilding customBuilding)
        {
            switch (Name)
            {
                case Type.InputItemNames:
                    return customBuilding.InputStorage.Value.items.Any(i => IsValid(i.Name));
                case Type.InputItemCount:
                    return IsValid(customBuilding.InputStorage.Value.items.Count(i => i is not null));
                case Type.OutputItemNames:
                    return customBuilding.OutputStorage.Value.items.Any(i => IsValid(i.Name));
                case Type.OutputItemCount:
                    return IsValid(customBuilding.OutputStorage.Value.items.Count(i => i is not null));
                case Type.HasFlag:
                    return customBuilding.Flags.Any(i => IsValid(i.Name));
                case Type.HasReceivedMail:
                    return Game1.MasterPlayer.mailReceived.Any(m => IsValid(m));
                case Type.HasSeenEvent:
                    return Game1.MasterPlayer.eventsSeen.Any(m => IsValid(m));
            }

            return false;
        }

        internal bool IsValid(bool booleanValue)
        {
            return IsValid(booleanValue, GetParsedValue<bool>(recalculateValue: false));
        }

        internal bool IsValid(bool booleanValue, bool comparisonValue)
        {
            var passed = (booleanValue == comparisonValue);
            if (Inverse)
            {
                passed = !passed;
            }

            return passed;
        }

        public bool IsValid(string stringValue)
        {
            var passed = false;
            var comparisonValue = GetParsedValue<string>(recalculateValue: false);
            switch (Operator)
            {
                case Comparison.Contains:
                    passed = stringValue.Contains(comparisonValue, StringComparison.OrdinalIgnoreCase);
                    break;
                default:
                    passed = stringValue.Equals(comparisonValue, StringComparison.OrdinalIgnoreCase);
                    break;
            }
            if (Inverse)
            {
                passed = !passed;
            }

            return passed;
        }

        internal bool IsValid(long numericalValue)
        {
            var passed = false;
            var comparisonValue = GetParsedValue<long>(recalculateValue: false);
            switch (Operator)
            {
                case Comparison.EqualTo:
                    passed = (numericalValue == comparisonValue);
                    break;
                case Comparison.GreaterThan:
                    passed = (numericalValue > comparisonValue);
                    break;
                case Comparison.LessThan:
                    passed = (numericalValue < comparisonValue);
                    break;
            }
            if (Inverse)
            {
                passed = !passed;
            }

            return passed;
        }

        internal T GetParsedValue<T>(bool recalculateValue = false)
        {
            if (ParsedValue is null || recalculateValue)
            {
                if (Value is JObject modelContext)
                {
                    if (modelContext["RandomRange"] != null)
                    {
                        var randomRange = JsonConvert.DeserializeObject<RandomRange>(modelContext["RandomRange"].ToString());

                        ParsedValue = (T)Convert.ChangeType(Game1.random.Next(randomRange.Min, randomRange.Max), typeof(T));
                    }
                    else if (modelContext["RandomValue"] != null)
                    {
                        var randomValue = JsonConvert.DeserializeObject<List<object>>(modelContext["RandomValue"].ToString());
                        ParsedValue = (T)Convert.ChangeType(randomValue[Game1.random.Next(randomValue.Count)], typeof(T));
                    }
                }
                else
                {
                    ParsedValue = Value;
                }
            }

            return (T)ParsedValue;
        }

        internal T GetCache<T>()
        {
            if (Cache is null)
            {
                return default;
            }

            return (T)Cache;
        }

        internal void SetCache(object value)
        {
            Cache = value;
        }
    }
}