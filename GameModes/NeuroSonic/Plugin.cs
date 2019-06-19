﻿using theori;

using NeuroSonic.Startup;
using System.IO;
using theori.IO;
using theori.Configuration;
using NeuroSonic.IO;
using theori.Resources;

namespace NeuroSonic
{
    internal static class Plugin
    {
        public const string NSC_CONFIG_FILE = "nsc-config.ini";

        public static string[] ProgramArgs { get; private set; }

        public static NscConfig Config { get; private set; }

        public static Gamepad Gamepad { get; private set; }

        public static ClientResourceManager DefaultSkin { get; private set; }

        /// <summary>
        /// Invoked when the plugin starts in Standalone.
        /// </summary>
        public static void NSC_Main(string[] args)
        {
            ProgramArgs = args;

            Host.OnUserQuit += NSC_Quit;

            Config = new NscConfig();
            if (File.Exists(NSC_CONFIG_FILE))
                LoadNscConfig();
            // save the defaults on init
            else SaveNscConfig();

            DefaultSkin = new ClientResourceManager("skins/Default", "materials/basic");
            DefaultSkin.AddManifestResourceLoader(ManifestResourceLoader.GetResourceLoader(typeof(Host).Assembly, "theori.Resources"));
            DefaultSkin.AddManifestResourceLoader(ManifestResourceLoader.GetResourceLoader(typeof(Plugin).Assembly, "NeuroSonic.Resources"));

            Gamepad = Gamepad.Open(Host.GameConfig.GetInt(GameConfigKey.Controller_DeviceID));
            Input.CreateController();

            // TODO(local): push the game loading layer, which creates the game layer
            //Host.PushLayer(new GameLayer(true));
            Host.PushLayer(new NeuroSonicStandaloneStartup());
        }

        private static void NSC_Quit()
        {
            SaveNscConfig();
        }

        public static void SwitchActiveGamepad(int newDeviceIndex)
        {
            if (newDeviceIndex == Gamepad.DeviceIndex) return;
            Gamepad.Close();

            Host.GameConfig.Set(GameConfigKey.Controller_DeviceID, newDeviceIndex);
            Gamepad = Gamepad.Open(newDeviceIndex);

            Input.CreateController();

            SaveNscConfig();
        }

        private static void LoadNscConfig()
        {
            using (var reader = new StreamReader(File.OpenRead(NSC_CONFIG_FILE)))
                Config.Load(reader);
        }

        public static void LoadConfig()
        {
            LoadNscConfig();
            Host.LoadConfig();
        }

        internal static void SaveNscConfig()
        {
            using (var writer = new StreamWriter(File.Open(NSC_CONFIG_FILE, FileMode.Create)))
                Config.Save(writer);
        }

        public static void SaveConfig()
        {
            SaveNscConfig();
            Host.SaveConfig();
        }
    }
}
