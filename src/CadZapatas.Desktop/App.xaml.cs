using System.Windows;

namespace CadZapatas.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Licencia Community de QuestPDF (para OSS).
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }
}
