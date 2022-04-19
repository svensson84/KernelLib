using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KernelLib
{
    public class Kernel
    { 
        public static void executeAsync(Action action)
        {
            // needs to be called from calling thread of public interface method #executeAsync
            SynchronizationContext? context = resolveThreadSyncContext(action);

            // decoupling of calling thread (async!)
            Thread backgroundThread = new Thread(() => {
                if (context is null)
                {
                    // run delegate as background thread
                    action.Invoke();
                }
                else
                {
                    // dispatch to thread in synchonization context
                    context.Post((object? input) => { action.Invoke(); }, null);
                }
            });
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
        }

        public static void executeAsync<TResult>(Func<TResult> function, Action<TResult> callback)
        {
            Debug.WriteLine("thread id={0} ; begin kernel.executeAsync()", Thread.CurrentThread.ManagedThreadId);

            // needs to be called from calling thread of public interface method #executeAsync
            SynchronizationContext? context = resolveThreadSyncContext(callback);

            // decoupling of calling thread (async!)
            Thread backgroundThread = new Thread(() => {
                Debug.WriteLine("backgroundThread id={0} spawned", Thread.CurrentThread.ManagedThreadId);
                Debug.WriteLine(context);

                TResult? result = function.Invoke();

                if (context is null)
                {
                    // run delegate as background thread
                    callback.Invoke(result);
                }
                else
                {
                    // dispatch to thread in synchonization context
                    context.Post((result) => { callback.Invoke((TResult)result); }, result);
                }

            });
            backgroundThread.IsBackground = true;
            backgroundThread.Start();

            Debug.WriteLine("thread id={0} ; end kernel.executeAsync()", Thread.CurrentThread.ManagedThreadId);
        }

        public static void executeAsync<TInput, TResult>(Func<TInput, TResult> function, TInput input, Action<TResult> callback)
        {
            // needs to be called from calling thread of public interface method #executeAsync
            SynchronizationContext? context = resolveThreadSyncContext(callback);

            // decoupling of calling thread (async!)
            Thread backgroundThread = new Thread(() => {

                TResult? result = function.Invoke(input);

                if (context is null)
                {
                    // run delegate as background thread
                    callback.Invoke(function.Invoke(input));
                }
                else
                {
                    // dispatch to thread in synchonization context
                    context.Post((result) => { callback.Invoke((TResult)result); }, result);
                }
            });
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
        }
        private static SynchronizationContext? resolveThreadSyncContext(Delegate anyDelegate)
        {
            return System.Attribute.IsDefined(anyDelegate.Method, typeof(UIThreadAccessible)) ? SynchronizationContext.Current : null;
        }

        public static void killAllThreadsFromCurrentAssembly() 
        {
            Process process = Process.GetCurrentProcess();
            ProcessThreadCollection threads = process.Threads;
            IEnumerable<ProcessThread> query = threads.Cast<ProcessThread>();

            foreach (var thread in from thread in query
                                   where thread.Id != GetUIThread(process)?.Id
                                   select new
                                   {
                                       thread.Id,
                                       thread.ThreadState
                                   })
            {
                // NOTE: 1 = stands for a constant value (access rights)
                IntPtr handle = OpenThread(1, false, (uint)thread.Id);
                //TerminateThread(handle, 0);
                SuspendThread(handle);
            }
        }

        public static ProcessThread GetUIThread(Process proc)
        {
            if (proc.MainWindowHandle == null)
                return null;

            int id = GetWindowThreadProcessId(proc.MainWindowHandle, IntPtr.Zero);

            foreach (ProcessThread pt in proc.Threads)
            {
                if (pt.Id == id)
                    return pt;
            }
            return null;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, IntPtr procid);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);
    }
}
