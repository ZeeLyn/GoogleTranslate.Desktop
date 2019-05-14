using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using GoogleTranslate.Desktop.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GoogleTranslate.Desktop
{
    public class TranslateModel : INotifyPropertyChanged, IDisposable
    {

        private InputQueue InputQueue { get; }

        private readonly RestClient _client = new RestClient("https://translate.google.cn");

        public TranslateModel()
        {
            InputQueue = new InputQueue();

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
            foreach (var item in AppSettingsManager.Read().RecentlyUsedLanguages)
            {
                RecentlyUsedLanguages.Add(item);
            }

            InputQueue.OnStopInput += async text =>
            {
                LastInputText = text;
                await Translate();
            };
        }

        private string _inputText;

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                InputQueue.In(value);
                OnPropertyChanged(nameof(InputText));
            }
        }


        public string LastInputText { get; set; }


        private string _translateResult;

        public string TranslateResult
        {
            get => _translateResult;
            set
            {
                _translateResult = value;
                OnPropertyChanged(nameof(TranslateResult));
            }
        }

        private string _targetLanguage = "en";
        public string TargetLanguage
        {
            get => _targetLanguage;
            set
            {
                _targetLanguage = value;
                AppSettingsManager.Read().CurrentTargetLanguage = value;
                AppSettingsManager.UpdateAppSettings();
                OnPropertyChanged(nameof(TargetLanguage));
            }
        }

        private string _targetLanguageText = "英语";
        public string TargetLanguageText
        {
            get => _targetLanguageText;
            set
            {
                _targetLanguageText = value;
                OnPropertyChanged(nameof(TargetLanguageText));
            }
        }

        public ICommand GenreDropDownMenuItemCommand { get; }


        public List<Language> Languages { get; set; }


        private List<MoreInformation> _moreInformation = new List<MoreInformation>();

        public List<MoreInformation> MoreInformation
        {
            get => _moreInformation;
            set
            {
                _moreInformation = value;
                OnPropertyChanged(nameof(MoreInformation));
            }
        }

        private ObservableCollection<Language> _recentlyUsedLanguages = new ObservableCollection<Language>();

        public ObservableCollection<Language> RecentlyUsedLanguages
        {
            get => _recentlyUsedLanguages;
            set
            {
                _recentlyUsedLanguages = value;
                OnPropertyChanged(nameof(RecentlyUsedLanguages));
            }
        }

        private int _showMore;

        public int ShowMore
        {
            get => _showMore;
            set
            {
                _showMore = value;
                OnPropertyChanged(nameof(ShowMore));
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
            if (RecentlyUsedLanguages.Any(p => p.Code == current.Code))
                return;
            if (RecentlyUsedLanguages.Count >= 5)
            {
                RecentlyUsedLanguages.RemoveAt(0);
            }
            RecentlyUsedLanguages.Insert(0, current);
            AppSettingsManager.Read().RecentlyUsedLanguages = RecentlyUsedLanguages.ToList();
            AppSettingsManager.UpdateAppSettings();
        }

        public async Task Translate()
        {
            if (string.IsNullOrWhiteSpace(LastInputText))
            {
                TranslateResult = "";
                MoreInformation = null;
                ShowMore = 0;
                return;
            }
            try
            {
                var tick = tk(LastInputText, "432558.706957580");
                var request = new RestRequest($"translate_a/single?client=webapp&ie=UTF-8&sl=auto&tl={TargetLanguage}&hl=zh-CN&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&dt=gt&pc=1&otf=1&ssel=0&tsel=0&kc=1&tk={tick}&q={HttpUtility.UrlEncode(LastInputText)}");
                var response = await _client.ExecuteGetTaskAsync(request);
                if (response.IsSuccessful)
                {
                    var json = JsonConvert.DeserializeObject<JArray>(response.Content);
                    var res = new StringBuilder();
                    if (json?[0] != null)
                    {
                        foreach (var item in json[0])
                        {
                            res.Append(item[0].Value<string>());
                        }
                        TranslateResult = res.ToString();
                    }

                    try
                    {
                        if (json?[1] != null && json[1].Any())
                        {
                            var result = new List<MoreInformation>();
                            foreach (var items in json[1])
                            {
                                var info = new MoreInformation
                                {
                                    WordAttribute = items[0].Value<string>(),
                                    WordToTranslates = new List<WordToTranslate>()
                                };
                                foreach (var words in items[2])
                                {
                                    var word = new WordToTranslate
                                    {
                                        Word = words[0].Value<string>(),
                                        Translates = new List<string>()
                                    };
                                    foreach (var translates in words[1])
                                    {
                                        word.Translates.Add(translates.Value<string>());
                                    }

                                    info.WordToTranslates.Add(word);
                                }

                                result.Add(info);
                            }

                            MoreInformation = result;
                        }
                        else
                            MoreInformation = null;

                        ShowMore =
                            MoreInformation != null && MoreInformation.Any()
                                ? 22
                                : 0;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch
            {
                // ignored
            }

        }
        #region
        private string b(long a, string b)
        {
            for (int d = 0; d < b.Length - 2; d += 3)
            {
                char c = b.charAt(d + 2);
                int c0 = 'a' <= c ? c.charCodeAt(0) - 87 : ExtensionMethods.Number(c);
                long c1 = '+' == b.charAt(d + 1) ? a >> c0 : a << c0;
                a = '+' == b.charAt(d) ? a + c1 & 4294967295 : a ^ c1;
            }
            a = ExtensionMethods.Number(a);
            return a.ToString();
        }

        private string tk(string a, string TKK)
        {
            string[] e = TKK.Split('.');
            int d = 0;
            int h = 0;
            h = ExtensionMethods.Number(e[0]);
            byte[] g0 = Encoding.UTF8.GetBytes(a);
            long aa = h;
            for (d = 0; d < g0.Length; d++)
            {
                aa += g0[d];
                aa = Convert.ToInt64(b(aa, "+-a^+6"));
            }
            aa = Convert.ToInt64(b(aa, "+-3^+b+-f"));
            long bb = aa ^ ExtensionMethods.Number(e[1]);
            aa = bb;
            aa = aa + bb;
            bb = aa - bb;
            aa = aa - bb;
            if (0 > aa)
            {
                aa = (aa & 2147483647) + 2147483648;
            }
            aa %= (long)1e6;
            return aa + "." + (aa ^ h);
        }
        #endregion

        public void Dispose()
        {
            InputQueue?.Dispose();
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


    public class MoreInformation
    {
        public string WordAttribute { get; set; }

        public List<WordToTranslate> WordToTranslates { get; set; }
    }

    public class WordToTranslate
    {
        public string Word { get; set; }

        public List<string> Translates { get; set; }
    }
}
