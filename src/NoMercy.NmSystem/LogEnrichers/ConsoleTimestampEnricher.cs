using System.Drawing;
using Pastel;
using Serilog.Core;
using Serilog.Events;

namespace NoMercy.NmSystem.LogEnrichers;
internal class ConsoleTimestampEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string? timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Pastel(Color.DarkGray);

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
            "Time", timestamp));
    }
}