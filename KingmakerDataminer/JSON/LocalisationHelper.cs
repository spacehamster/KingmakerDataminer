using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomBlueprints
{
    public class LocalizationHelper
    {
        static LocalizationPack Pack;
        public static LocalizationPack LoadPack(string path, Locale locale)
        {
            path = Path.Combine(ApplicationPaths.streamingAssetsPath, path);
            if (File.Exists(path))
            {
                try
                {
                    JsonSerializer jsonSerializer = JsonSerializer.Create(DefaultJsonSettings.DefaultSettings);
                    using (StreamReader streamReader = new StreamReader(path))
                    {
                            LocalizationPack localizationPack = null;
                            using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                            {
                                localizationPack = jsonSerializer.Deserialize<LocalizationPack>(jsonTextReader);
                                localizationPack.Locale = locale;
                            }
                            //Main.DebugLog("Loaded localization pack " + locale);
                            return localizationPack;
                    }
                }
                catch (Exception ex)
                {
                    Main.DebugLog("Failed to load localization pack " + path);
                    Main.DebugLog(ex.ToString());
                    return null;
                }
            }
            return null;
        }
        public static void Init(Locale locale)
        {
            var pack = new LocalizationPack();
            pack.Locale = locale;
            pack.Strings = new Dictionary<string, string>(LocalizationManager.CurrentPack.Strings);
            foreach(var dialog in ResourcesLibrary.GetBlueprints<BlueprintDialog>())
            {
                var dialogPack = LoadPack($"Localization/{locale}{dialog.AssetGuid}.json", locale);
                if (dialogPack == null) continue;
                foreach(var kv in dialogPack.Strings)
                {
                    pack.Strings[kv.Key] = kv.Value;
                }
            }
            Pack = pack;

        }
        public static string GetText(string key)
        {
            if (Pack == null) Init(LocalizationManager.CurrentLocale);
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }
            return Pack.GetText(key);
        }
    }
}
