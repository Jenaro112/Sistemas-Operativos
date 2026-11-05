using System.Globalization;
using System.Text;

namespace Lamport.Common;

/// <summary>
/// Registro de ejecución: escribe cada línea en consola (docker compose logs)
/// y en un archivo logs/&lt;nombre&gt;.log para adjuntar como evidencia.
/// </summary>
public static class ExecLog
{
    private static readonly object Lock = new();
    private static string? _file;
    private static string _name = "app";

    public static void Init(string name)
    {
        _name = name;
        var dir = Environment.GetEnvironmentVariable("LOG_DIR") ?? "logs";
        Directory.CreateDirectory(dir);
        _file = Path.Combine(dir, $"{name}.log");
        Console.OutputEncoding = Encoding.UTF8;
    }

    public static void Write(string message)
    {
        var lines = message.Split('\n');
        lock (Lock)
        {
            foreach (var l in lines)
            {
                var line = $"[{_name}] {l.TrimEnd('\r')}";
                Console.WriteLine(line);
                if (_file is not null) File.AppendAllText(_file, line + Environment.NewLine);
            }
        }
    }
}

/// <summary>Formato de horas para mostrar (UTC-3 por defecto, configurable con DISPLAY_UTC_OFFSET_HOURS).</summary>
public static class Fmt
{
    private static readonly double DisplayOffsetHours =
        double.TryParse(Environment.GetEnvironmentVariable("DISPLAY_UTC_OFFSET_HOURS"),
            NumberStyles.Float, CultureInfo.InvariantCulture, out var h) ? h : -3;

    public static string Time(DateTime utc) =>
        utc.AddHours(DisplayOffsetHours).ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

    public static string Ms(double ms) =>
        (ms >= 0 ? "+" : "") + ms.ToString("0.0", CultureInfo.InvariantCulture) + " ms";

    public static string Ms(TimeSpan ts) => Ms(ts.TotalMilliseconds);
}

public static class Env
{
    public static string Str(string key, string def) =>
        Environment.GetEnvironmentVariable(key) is { Length: > 0 } v ? v : def;

    public static double Num(string key, double def) =>
        double.TryParse(Environment.GetEnvironmentVariable(key), NumberStyles.Float,
            CultureInfo.InvariantCulture, out var v) ? v : def;

    public static int Int(string key, int def) => (int)Num(key, def);
}
