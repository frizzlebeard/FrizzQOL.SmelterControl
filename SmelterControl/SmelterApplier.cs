using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SmelterControl
{
    internal static class SmelterApplier
    {
        private struct VanillaStats
        {
            public float Seconds;
            public int Ore;
            public int Fuel;
        }

        private static readonly Dictionary<string, VanillaStats> Vanilla = new Dictionary<string, VanillaStats>();

        internal static void Apply(Smelter smelter)
        {
            if (smelter == null)
            {
                return;
            }

            string objectName = smelter.gameObject != null ? smelter.gameObject.name : null;
            ApplyNamed(smelter, SmelterRules.PrefabName(objectName));
        }

        internal static void ApplyPrefabs(ZNetScene scene)
        {
            if (scene == null)
            {
                return;
            }

            List<GameObject> prefabs = AccessTools.Field(typeof(ZNetScene), "m_prefabs")?.GetValue(scene) as List<GameObject>;
            if (prefabs == null)
            {
                Plugin.LogOnce("ZNetScene prefab list was missing. Smelter config was not applied.");
                return;
            }

            bool sawSmelter = false;
            bool sawBlast = false;
            bool sawKiln = false;
            for (int i = 0; i < prefabs.Count; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                string name = SmelterRules.PrefabName(prefab.name);
                if (!SmelterRules.IsControlled(name))
                {
                    continue;
                }

                Smelter smelter = prefab.GetComponent<Smelter>();
                if (name == SmelterRules.SmelterPrefab)
                {
                    sawSmelter |= smelter != null;
                }
                else if (name == SmelterRules.BlastFurnacePrefab)
                {
                    sawBlast |= smelter != null;
                }
                else if (name == SmelterRules.CharcoalKilnPrefab)
                {
                    sawKiln |= smelter != null;
                }

                ApplyNamed(smelter, name);
            }

            if (!sawSmelter)
            {
                Plugin.LogOnce("Missing smelter prefab: " + SmelterRules.SmelterPrefab);
            }

            if (!sawBlast)
            {
                Plugin.LogOnce("Missing smelter prefab: " + SmelterRules.BlastFurnacePrefab);
            }

            if (!sawKiln)
            {
                Plugin.LogOnce("Missing smelter prefab: " + SmelterRules.CharcoalKilnPrefab);
            }
        }

        internal static void ApplyAll()
        {
            if (ZNetScene.instance != null)
            {
                ApplyPrefabs(ZNetScene.instance);
            }

            Smelter[] loaded = Object.FindObjectsByType<Smelter>(FindObjectsSortMode.None);
            for (int i = 0; i < loaded.Length; i++)
            {
                Apply(loaded[i]);
            }
        }

        private static void ApplyNamed(Smelter smelter, string prefabName)
        {
            if (smelter == null || !SmelterRules.IsControlled(prefabName))
            {
                return;
            }

            if (!Vanilla.ContainsKey(prefabName))
            {
                Vanilla[prefabName] = new VanillaStats
                {
                    Seconds = smelter.m_secPerProduct,
                    Ore = smelter.m_maxOre,
                    Fuel = smelter.m_maxFuel
                };
            }

            VanillaStats vanilla = Vanilla[prefabName];
            Plugin.StationSettings settings = Plugin.ConfigFor(prefabName);
            smelter.m_secPerProduct = SmelterRules.ResolveSeconds(settings.Seconds, vanilla.Seconds);
            smelter.m_maxOre = SmelterRules.ResolveCount(settings.Input, vanilla.Ore);
            if (SmelterRules.WritesFuel(prefabName, smelter.m_fuelItem != null))
            {
                smelter.m_maxFuel = SmelterRules.ResolveCount(settings.Fuel, vanilla.Fuel);
            }
        }
    }
}
