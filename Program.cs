using System.Diagnostics;

// Get total physical memory
long totalRam = GetTotalPhysicalMemory();

// Get all processes and their memory usage
var processes = Process.GetProcesses()
    .Select(p =>
    {
        try
        {
            return new ProcessInfo(p.ProcessName, p.Id, p.WorkingSet64);
        }
        catch
        {
            return null;
        }
        finally
        {
            p.Dispose();
        }
    })
    .OfType<ProcessInfo>()
    .OrderByDescending(p => p.WorkingSet)
    .ToList();

// Print header
Console.WriteLine();
Console.WriteLine("Windows, huh? Here's what's going on with your RAM:");
Console.WriteLine($"  Total RAM: {FormatBytes(totalRam)}");
Console.WriteLine();
Console.WriteLine($"{"Process",-40} {"PID",6}  {"RAM Usage",12}  {"% of RAM",8}");
Console.WriteLine(new string('-', 74));

// Print each process
foreach (var proc in processes)
{
    double pct = totalRam > 0 ? proc.WorkingSet * 100.0 / totalRam : 0;
    string pctStr = pct >= 0.1 ? $"{pct:F1}%" : "<0.1%";
    Console.WriteLine($"{proc.Name,-40} {proc.Pid,6}  {FormatBytes(proc.WorkingSet),12}  {pctStr,8}");
}

Console.WriteLine();
long totalUsed = processes.Sum(p => p.WorkingSet);
double totalPct = totalRam > 0 ? totalUsed * 100.0 / totalRam : 0;
Console.WriteLine($"Total working set across {processes.Count} processes: {FormatBytes(totalUsed)} ({totalPct:F1}% of RAM)");
Console.WriteLine();

static long GetTotalPhysicalMemory()
{
    // GC.GetGCMemoryInfo().TotalAvailableMemoryBytes reflects total physical RAM
    // on most platforms (.NET 5+)
    return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
}

static string FormatBytes(long bytes)
{
    if (bytes >= 1024L * 1024 * 1024)
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    if (bytes >= 1024 * 1024)
        return $"{bytes / (1024.0 * 1024):F1} MB";
    if (bytes >= 1024)
        return $"{bytes / 1024.0:F1} KB";
    return $"{bytes} B";
}

record ProcessInfo(string Name, int Pid, long WorkingSet);
