using System.Diagnostics;

namespace PlatformService.OpenTelemtry;

public static class DiagnosticsConfig
{
    public const string SourceName = "platformservice";
    public static ActivitySource Source = new ActivitySource(SourceName);
}