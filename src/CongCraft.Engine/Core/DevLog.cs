using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CongCraft.Engine.Core;

/// <summary>
/// Lightweight file+console logger for diagnosing startup crashes (especially on macOS).
/// Writes to CongCraft.log next to the executable.
/// </summary>
public static class DevLog
{
    private static readonly object Lock = new();
    private static StreamWriter? _writer;
    private static readonly StringBuilder _buffer = new();
    private static bool _initialized;

    public static string LogFilePath { get; private set; } = "CongCraft.log";

    public static void Init()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            // Log file next to executable
            var exeDir = AppContext.BaseDirectory;
            LogFilePath = Path.Combine(exeDir, "CongCraft.log");
            _writer = new StreamWriter(LogFilePath, append: false, Encoding.UTF8)
            {
                AutoFlush = true
            };
        }
        catch
        {
            // Fallback: temp directory
            try
            {
                LogFilePath = Path.Combine(Path.GetTempPath(), "CongCraft.log");
                _writer = new StreamWriter(LogFilePath, append: false, Encoding.UTF8)
                {
                    AutoFlush = true
                };
            }
            catch
            {
                // Can't write logs at all — just use console
            }
        }

        Info("=== CongCraft DevLog started ===");
        Info($"Time:     {DateTime.UtcNow:u}");
        Info($"OS:       {RuntimeInformation.OSDescription}");
        Info($"Arch:     {RuntimeInformation.OSArchitecture}");
        Info($"Runtime:  {RuntimeInformation.FrameworkDescription}");
        Info($"Process:  {RuntimeInformation.ProcessArchitecture}");
        Info($"AOT:      {!RuntimeFeature.IsDynamicCodeSupported}");
        Info($"LogFile:  {LogFilePath}");
    }

    public static void Info(string message)
    {
        Write("INFO", message);
    }

    public static void Warn(string message)
    {
        Write("WARN", message);
    }

    public static void Error(string message)
    {
        Write("ERROR", message);
    }

    public static void Error(string message, Exception ex)
    {
        Write("ERROR", $"{message}: {ex}");
    }

    public static void Section(string title)
    {
        Write("----", $"--- {title} ---");
    }

    private static void Write(string level, string message)
    {
        var line = $"[{DateTime.UtcNow:HH:mm:ss.fff}] [{level}] {message}";
        lock (Lock)
        {
            Console.WriteLine(line);
            try
            {
                _writer?.WriteLine(line);
            }
            catch
            {
                // Ignore write failures
            }
        }
    }

    public static void Shutdown()
    {
        Info("=== CongCraft DevLog shutdown ===");
        lock (Lock)
        {
            _writer?.Flush();
            _writer?.Dispose();
            _writer = null;
        }
    }
}
