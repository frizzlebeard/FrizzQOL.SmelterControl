using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace SmelterControl
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.frizzqol.smeltercontrol";
        public const string PluginName = "FrizzQOL Smelter Control";
        public const string PluginVersion = "0.1.0";

        internal static Plugin Instance { get; private set; }

        internal struct StationSettings
        {
            public float Seconds;
            public int Input;
            public int Fuel;
        }

        internal static ConfigEntry<float> SmelterSeconds;
        internal static ConfigEntry<int> SmelterOre;
        internal static ConfigEntry<int> SmelterFuel;
        internal static ConfigEntry<float> BlastSeconds;
        internal static ConfigEntry<int> BlastOre;
        internal static ConfigEntry<int> BlastFuel;
        internal static ConfigEntry<float> KilnSeconds;
        internal static ConfigEntry<int> KilnWood;

        private static readonly HashSet<string> Logged = new HashSet<string>();
        private const long ConfigReloadQuietTicks = 250L * TimeSpan.TicksPerMillisecond;
        private Harmony _harmony;
        private FileSystemWatcher _configWatcher;
        private long _configQuietUntilTicks;
        private int _applyQueued;

        private void Awake()
        {
            Instance = this;
            SmelterSeconds = Config.Bind("Smelter", "Seconds", 0f, "Seconds per bar. 0 keeps the normal 30. Below 0 does the same.");
            SmelterOre = Config.Bind("Smelter", "Ore", 0, "Ore the smelter will hold. 0 keeps the normal 10. Below 0 does the same.");
            SmelterFuel = Config.Bind("Smelter", "Fuel", 0, "Coal the smelter will hold. 0 keeps the normal 20. Below 0 does the same.");
            BlastSeconds = Config.Bind("BlastFurnace", "Seconds", 0f, "Seconds per bar. 0 keeps the normal 30. Below 0 does the same.");
            BlastOre = Config.Bind("BlastFurnace", "Ore", 0, "Ore the blast furnace will hold. 0 keeps the normal 10. Below 0 does the same.");
            BlastFuel = Config.Bind("BlastFurnace", "Fuel", 0, "Coal the blast furnace will hold. 0 keeps the normal 20. Below 0 does the same.");
            KilnSeconds = Config.Bind("CharcoalKiln", "Seconds", 0f, "Seconds per coal. 0 keeps the normal 15. Below 0 does the same.");
            KilnWood = Config.Bind("CharcoalKiln", "Wood", 0, "Wood the kiln will hold. 0 keeps the normal 25. Below 0 does the same.");
            Config.SettingChanged += (_, __) => QueueApply();
            WatchConfigFile();

            _harmony = new Harmony(PluginGuid);
            try
            {
                _harmony.PatchAll();
                Logger.LogInfo(PluginName + " " + PluginVersion + " loaded");
            }
            catch (Exception ex)
            {
                Logger.LogError("Harmony patch failed: " + ex.ToString());
                _harmony.UnpatchSelf();
            }
        }

        private void QueueApply()
        {
            if (Interlocked.CompareExchange(ref _applyQueued, 1, 0) != 0)
            {
                return;
            }

            if (ThreadingHelper.Instance != null)
            {
                ThreadingHelper.Instance.StartSyncInvoke(ApplyQueued);
                return;
            }

            ApplyQueued();
        }

        private void ApplyQueued()
        {
            Interlocked.Exchange(ref _applyQueued, 0);
            SmelterApplier.ApplyAll();
        }

        private void WatchConfigFile()
        {
            string path = Config.ConfigFilePath;
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            {
                return;
            }

            _configWatcher = new FileSystemWatcher(directory, fileName);
            _configWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _configWatcher.Changed += OnConfigFileChanged;
            _configWatcher.EnableRaisingEvents = true;
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs args)
        {
            long now = DateTime.UtcNow.Ticks;
            long quietUntil = Interlocked.Read(ref _configQuietUntilTicks);
            if (now < quietUntil)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _configQuietUntilTicks, now + ConfigReloadQuietTicks, quietUntil) != quietUntil)
            {
                return;
            }

            if (ThreadingHelper.Instance != null)
            {
                ThreadingHelper.Instance.StartSyncInvoke(ReloadConfigFile);
                return;
            }

            ReloadConfigFile();
        }

        private void ReloadConfigFile()
        {
            Config.Reload();
        }

        internal static StationSettings ConfigFor(string prefabName)
        {
            if (prefabName == SmelterRules.BlastFurnacePrefab)
            {
                return new StationSettings
                {
                    Seconds = ReadSeconds(BlastSeconds, "BlastFurnace Seconds"),
                    Input = ReadCount(BlastOre, "BlastFurnace Ore"),
                    Fuel = ReadCount(BlastFuel, "BlastFurnace Fuel")
                };
            }

            if (prefabName == SmelterRules.CharcoalKilnPrefab)
            {
                return new StationSettings
                {
                    Seconds = ReadSeconds(KilnSeconds, "CharcoalKiln Seconds"),
                    Input = ReadCount(KilnWood, "CharcoalKiln Wood"),
                    Fuel = 0
                };
            }

            if (prefabName == SmelterRules.SmelterPrefab)
            {
                return new StationSettings
                {
                    Seconds = ReadSeconds(SmelterSeconds, "Smelter Seconds"),
                    Input = ReadCount(SmelterOre, "Smelter Ore"),
                    Fuel = ReadCount(SmelterFuel, "Smelter Fuel")
                };
            }

            return new StationSettings
            {
                Seconds = 0f,
                Input = 0,
                Fuel = 0
            };
        }

        internal static void LogOnce(string message)
        {
            if (string.IsNullOrEmpty(message) || Logged.Contains(message))
            {
                return;
            }

            Logged.Add(message);
            LogWarning(message);
        }

        internal static void LogError(string message)
        {
            if (Instance != null)
            {
                Instance.Logger.LogError(message);
            }
        }

        private static float ReadSeconds(ConfigEntry<float> entry, string label)
        {
            float value = entry != null ? entry.Value : 0f;
            if (value < 0f)
            {
                LogOnce(label + " below 0 keeps the normal value.");
            }

            return value;
        }

        private static int ReadCount(ConfigEntry<int> entry, string label)
        {
            int value = entry != null ? entry.Value : 0;
            if (value < 0)
            {
                LogOnce(label + " below 0 keeps the normal value.");
            }

            return value;
        }

        private static void LogWarning(string message)
        {
            if (Instance != null)
            {
                Instance.Logger.LogWarning(message);
            }
        }
    }
}
