using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Pes2019MlEditor.Tests;

public class MemoryScannerTests
{
    private readonly ITestOutputHelper _output;
    public MemoryScannerTests(ITestOutputHelper output) => _output = output;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [Fact]
    public void CheckAccess()
    {
        var proc = Process.GetProcessesByName("PES2019").FirstOrDefault();
        if (proc == null) return;

        // Try different access rights
        uint[] accesses = [0x0410, 0x0010, 0x1F0FFF, 0x1000];
        foreach (var acc in accesses)
        {
            IntPtr h = OpenProcess(acc, false, proc.Id);
            int err = Marshal.GetLastWin32Error();
            _output.WriteLine($"Access 0x{acc:X4} -> Handle: {h} (Win32Error: {err})");
        }
    }
}
