using KernelLib;
using System.Diagnostics;
using static System.Diagnostics.Debug;

Process process = Process.GetCurrentProcess();
ProcessThreadCollection threads = process.Threads;
IEnumerable<ProcessThread> query = threads.Cast<ProcessThread>();

foreach (var thread in from thread in query
                       where thread.Id != Kernel.GetUIThread(process)?.Id
                       select new
                       {
                           thread.Id,
                           thread.ThreadState
                       })
{
    WriteLine($"{thread.Id} {thread.ThreadState}");
}