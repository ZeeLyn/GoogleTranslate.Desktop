using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GoogleTranslate.Desktop.Annotations;
using MahApps.Metro.Controls;
using Newtonsoft.Json;

namespace GoogleTranslate.Desktop
{
    public class TranslateModel : INotifyPropertyChanged
    {

        public TranslateModel()
        {

            TargetLanguages.Seed();
            this.Languages = TargetLanguages.Languages;

            GenreDropDownMenuItemCommand = new GeneralCommand(o => true, x =>
            {
                var selectedItem = (Language)x;
                this.TargetLanguage = selectedItem.Code;
                this.TargetLanguageText = selectedItem.Name;
                OnChangeTargetLanguage?.Invoke(selectedItem);

                UpdateRecentlyUsedLanguages(selectedItem);
            });
            ReadRecentlyUsedLanguages();
        }

        private string _inputText;

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
            }
        }


        private string _translateResult;

        public string TranslateResult
        {
            get => _translateResult;
            set
            {
                _translateResult = value;
                OnPropertyChanged();
            }
        }

        private string _targetLanguage = "en";
        public string TargetLanguage
        {
            get => _targetLanguage;
            set
            {
                _targetLanguage = value;
                OnPropertyChanged();
            }
        }

        private string _targetLanguageText = "英语";
        public string TargetLanguageText
        {
            get => _targetLanguageText;
            set
            {
                _targetLanguageText = value;
                OnPropertyChanged();
            }
        }

        public ICommand GenreDropDownMenuItemCommand { get; }


        public List<Language> Languages { get; set; }

        private List<Language> _recentlyUsedLanguages;
        public List<Language> RecentlyUsedLanguages
        {
            get => _recentlyUsedLanguages;
            set
            {
                _recentlyUsedLanguages = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event GeneralCommandHandler OnChangeTargetLanguage;

        private void UpdateRecentlyUsedLanguages(Language current)
        {

            if (RecentlyUsedLanguages == null)
            {
                RecentlyUsedLanguages = new List<Language> { current };
            }
            else
            {

                if (RecentlyUsedLanguages.Any(p => p.Code == current.Code))
                    return;
                if (RecentlyUsedLanguages.Count >= 5)
                {
                    RecentlyUsedLanguages.RemoveAt(RecentlyUsedLanguages.Count - 1);
                }

                RecentlyUsedLanguages.Add(current);
            }
            var configUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            File.WriteAllText(configUrl, JsonConvert.SerializeObject(RecentlyUsedLanguages));
        }

        private void ReadRecentlyUsedLanguages()
        {
            var configUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            if (File.Exists(configUrl))
            {
                var json = File.ReadAllText(configUrl);
                if (string.IsNullOrWhiteSpace(json))
                    return;
                try
                {
                    RecentlyUsedLanguages = JsonConvert.DeserializeObject<List<Language>>(json);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

    public static class TargetLanguages
    {
        public static List<Language> Languages { get; set; }

        public static void Seed()
        {
            var json = "[{\"code\":\"en\",\"name\":\"英语\"},{\"code\":\"zh-CN\",\"name\":\"中文(简体)\"},{\"code\":\"sq\",\"name\":\"阿尔巴尼亚语\"},{\"code\":\"ar\",\"name\":\"阿拉伯语\"},{\"code\":\"am\",\"name\":\"阿姆哈拉语\"},{\"code\":\"az\",\"name\":\"阿塞拜疆语\"},{\"code\":\"ga\",\"name\":\"爱尔兰语\"},{\"code\":\"et\",\"name\":\"爱沙尼亚语\"},{\"code\":\"eu\",\"name\":\"巴斯克语\"},{\"code\":\"be\",\"name\":\"白俄罗斯语\"},{\"code\":\"bg\",\"name\":\"保加利亚语\"},{\"code\":\"is\",\"name\":\"冰岛语\"},{\"code\":\"pl\",\"name\":\"波兰语\"},{\"code\":\"bs\",\"name\":\"波斯尼亚语\"},{\"code\":\"fa\",\"name\":\"波斯语\"},{\"code\":\"af\",\"name\":\"布尔语(南非荷兰语)\"},{\"code\":\"da\",\"name\":\"丹麦语\"},{\"code\":\"de\",\"name\":\"德语\"},{\"code\":\"ru\",\"name\":\"俄语\"},{\"code\":\"fr\",\"name\":\"法语\"},{\"code\":\"tl\",\"name\":\"菲律宾语\"},{\"code\":\"fi\",\"name\":\"芬兰语\"},{\"code\":\"fy\",\"name\":\"弗里西语\"},{\"code\":\"km\",\"name\":\"高棉语\"},{\"code\":\"ka\",\"name\":\"格鲁吉亚语\"},{\"code\":\"gu\",\"name\":\"古吉拉特语\"},{\"code\":\"kk\",\"name\":\"哈萨克语\"},{\"code\":\"ht\",\"name\":\"海地克里奥尔语\"},{\"code\":\"ko\",\"name\":\"韩语\"},{\"code\":\"ha\",\"name\":\"豪萨语\"},{\"code\":\"nl\",\"name\":\"荷兰语\"},{\"code\":\"ky\",\"name\":\"吉尔吉斯语\"},{\"code\":\"gl\",\"name\":\"加利西亚语\"},{\"code\":\"ca\",\"name\":\"加泰罗尼亚语\"},{\"code\":\"cs\",\"name\":\"捷克语\"},{\"code\":\"kn\",\"name\":\"卡纳达语\"},{\"code\":\"co\",\"name\":\"科西嘉语\"},{\"code\":\"hr\",\"name\":\"克罗地亚语\"},{\"code\":\"ku\",\"name\":\"库尔德语\"},{\"code\":\"la\",\"name\":\"拉丁语\"},{\"code\":\"lv\",\"name\":\"拉脱维亚语\"},{\"code\":\"lo\",\"name\":\"老挝语\"},{\"code\":\"lt\",\"name\":\"立陶宛语\"},{\"code\":\"lb\",\"name\":\"卢森堡语\"},{\"code\":\"ro\",\"name\":\"罗马尼亚语\"},{\"code\":\"mg\",\"name\":\"马尔加什语\"},{\"code\":\"mt\",\"name\":\"马耳他语\"},{\"code\":\"mr\",\"name\":\"马拉地语\"},{\"code\":\"ml\",\"name\":\"马拉雅拉姆语\"},{\"code\":\"ms\",\"name\":\"马来语\"},{\"code\":\"mk\",\"name\":\"马其顿语\"},{\"code\":\"mi\",\"name\":\"毛利语\"},{\"code\":\"mn\",\"name\":\"蒙古语\"},{\"code\":\"bn\",\"name\":\"孟加拉语\"},{\"code\":\"my\",\"name\":\"缅甸语\"},{\"code\":\"hmn\",\"name\":\"苗语\"},{\"code\":\"xh\",\"name\":\"南非科萨语\"},{\"code\":\"zu\",\"name\":\"南非祖鲁语\"},{\"code\":\"ne\",\"name\":\"尼泊尔语\"},{\"code\":\"no\",\"name\":\"挪威语\"},{\"code\":\"pa\",\"name\":\"旁遮普语\"},{\"code\":\"pt\",\"name\":\"葡萄牙语\"},{\"code\":\"ps\",\"name\":\"普什图语\"},{\"code\":\"ny\",\"name\":\"齐切瓦语\"},{\"code\":\"ja\",\"name\":\"日语\"},{\"code\":\"sv\",\"name\":\"瑞典语\"},{\"code\":\"sm\",\"name\":\"萨摩亚语\"},{\"code\":\"sr\",\"name\":\"塞尔维亚语\"},{\"code\":\"st\",\"name\":\"塞索托语\"},{\"code\":\"si\",\"name\":\"僧伽罗语\"},{\"code\":\"eo\",\"name\":\"世界语\"},{\"code\":\"sk\",\"name\":\"斯洛伐克语\"},{\"code\":\"sl\",\"name\":\"斯洛文尼亚语\"},{\"code\":\"sw\",\"name\":\"斯瓦希里语\"},{\"code\":\"gd\",\"name\":\"苏格兰盖尔语\"},{\"code\":\"ceb\",\"name\":\"宿务语\"},{\"code\":\"so\",\"name\":\"索马里语\"},{\"code\":\"tg\",\"name\":\"塔吉克语\"},{\"code\":\"te\",\"name\":\"泰卢固语\"},{\"code\":\"ta\",\"name\":\"泰米尔语\"},{\"code\":\"th\",\"name\":\"泰语\"},{\"code\":\"tr\",\"name\":\"土耳其语\"},{\"code\":\"cy\",\"name\":\"威尔士语\"},{\"code\":\"ur\",\"name\":\"乌尔都语\"},{\"code\":\"uk\",\"name\":\"乌克兰语\"},{\"code\":\"uz\",\"name\":\"乌兹别克语\"},{\"code\":\"es\",\"name\":\"西班牙语\"},{\"code\":\"iw\",\"name\":\"希伯来语\"},{\"code\":\"el\",\"name\":\"希腊语\"},{\"code\":\"haw\",\"name\":\"夏威夷语\"},{\"code\":\"sd\",\"name\":\"信德语\"},{\"code\":\"hu\",\"name\":\"匈牙利语\"},{\"code\":\"sn\",\"name\":\"修纳语\"},{\"code\":\"hy\",\"name\":\"亚美尼亚语\"},{\"code\":\"ig\",\"name\":\"伊博语\"},{\"code\":\"it\",\"name\":\"意大利语\"},{\"code\":\"yi\",\"name\":\"意第绪语\"},{\"code\":\"hi\",\"name\":\"印地语\"},{\"code\":\"su\",\"name\":\"印尼巽他语\"},{\"code\":\"id\",\"name\":\"印尼语\"},{\"code\":\"jw\",\"name\":\"印尼爪哇语\"},{\"code\":\"yo\",\"name\":\"约鲁巴语\"},{\"code\":\"vi\",\"name\":\"越南语\"},{\"code\":\"zh-TW\",\"name\":\"中文(繁体)\"}]";
            Languages = JsonConvert.DeserializeObject<List<Language>>(json);
        }
    }

    public class Language
    {
        public Language(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public string Code { get; set; }

        public string Name { get; set; }
    }
}
