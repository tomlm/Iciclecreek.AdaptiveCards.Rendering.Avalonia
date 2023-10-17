using System.Runtime.Versioning;
using System.Threading.Tasks;

using AdaptiveCardViewer;

using Avalonia;
using Avalonia.Browser;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main(string[] args) => await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
