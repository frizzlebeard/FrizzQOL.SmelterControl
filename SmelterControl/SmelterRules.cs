namespace SmelterControl
{
    public static class SmelterRules
    {
        public const string SmelterPrefab = "piece_smelter";
        public const string BlastFurnacePrefab = "piece_blastfurnace";
        public const string CharcoalKilnPrefab = "piece_charcoalkiln";

        public static float ResolveSeconds(float configured, float vanilla)
        {
            if (configured <= 0f)
            {
                return vanilla;
            }

            return configured;
        }

        public static int ResolveCount(int configured, int vanilla)
        {
            if (configured <= 0)
            {
                return vanilla;
            }

            return configured;
        }

        public static bool IsControlled(string prefabName)
        {
            return prefabName == SmelterPrefab
                || prefabName == BlastFurnacePrefab
                || prefabName == CharcoalKilnPrefab;
        }

        public static bool WritesFuel(string prefabName, bool hasFuelItem)
        {
            if (!hasFuelItem)
            {
                return false;
            }

            return prefabName == SmelterPrefab || prefabName == BlastFurnacePrefab;
        }

        public static bool OreIsFull(int queueSize, int maxOre)
        {
            return queueSize >= maxOre;
        }

        public static bool FuelIsFull(float fuel, int maxFuel)
        {
            return fuel > maxFuel - 1;
        }

        public static string PrefabName(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return "";
            }

            int clone = objectName.IndexOf("(Clone)");
            return clone >= 0 ? objectName.Substring(0, clone).Trim() : objectName;
        }
    }
}
