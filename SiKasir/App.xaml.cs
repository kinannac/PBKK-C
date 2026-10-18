using System.Windows;
using SiKasir.Repositories;
using SiKasir.Views.Auth;
using SiKasir.Views.Installer;

namespace SiKasir
{
    public partial class App : Application
    {
        public App()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage("id-ID")));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (DatabaseConnection.IsConfigured) new LoginWindow().Show();
            else new InstallerWindow().Show();
        }
    }
}