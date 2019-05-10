using System;
using System.Windows;
using MahApps.Metro;

namespace GoogleTranslate.Desktop
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.AddAccent("Dark", new Uri("pack://application:,,,/GoogleTranslate.Desktop;component/CustomThemes.xaml"));

            // get the current app style (theme and accent) from the application
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);

            // now change app style to the custom accent and current theme
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent("Dark"),
                theme.Item1);
            base.OnStartup(e);
        }
    }
}
