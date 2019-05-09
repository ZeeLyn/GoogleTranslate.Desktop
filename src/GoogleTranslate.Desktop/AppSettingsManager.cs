using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GoogleTranslate.Desktop
{
    public class AppSettingsManager
    {
        private static AppSettings AppSettings { get; set; }

        public static AppSettings Read()
        {
            if (AppSettings != null) return AppSettings;
            var configUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            if (File.Exists(configUrl))
            {
                var json = File.ReadAllText(configUrl);
                if (string.IsNullOrWhiteSpace(json))
                    return AppSettings = new AppSettings();
                try
                {
                    return AppSettings = JsonConvert.DeserializeObject<AppSettings>(json);
                }
                catch
                {
                    return AppSettings = new AppSettings();
                }
            }
            return AppSettings = new AppSettings();
        }

        public static void UpdateAppSettings()
        {
            var configUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            File.WriteAllText(configUrl, JsonConvert.SerializeObject(AppSettings));
        }
    }

    public class AppSettings
    {
        public string CurrentTargetLanguage { get; set; } = "en";

        public bool TopMost { get; set; }

        public List<Language> RecentlyUsedLanguages { get; set; } = new List<Language>();
    }
}
