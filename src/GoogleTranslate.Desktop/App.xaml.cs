using System;
using System.Collections.Generic;
using System.Windows;
using MahApps.Metro;
using Microsoft.Shell;

namespace GoogleTranslate.Desktop
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string Unique = "3BF17EF9-344B-4880-AB2C-8ECDB7860485";

        [STAThread]
        public static void Main()
        {
            if (!SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                MessageBox.Show("已运行了一个示例，使用快捷键 Control+` 唤起.", "提示");
                return;
            }

            var application = new App();
            application.InitializeComponent();
            application.Run();
            SingleInstance<App>.Cleanup();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.AddAccent("Dark", new Uri("pack://application:,,,/GoogleTranslate.Desktop;component/CustomThemes.xaml"));
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent("Dark"),
                theme.Item1);
            base.OnStartup(e);
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            return true;
        }
    }
}
