using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ClearLog
{
    class Program
    {
        static void Main(string[] args)
        {
            // 检查是否以管理员身份运行
            if (!IsRunAsAdmin())
            {
                Console.WriteLine("请以管理员身份运行该程序。");
                return;
            }

            // 获取并清除传统事件日志
            var eventLogs = EventLog.GetEventLogs();
            foreach (var log in eventLogs)
            {
                try
                {
                    log.Clear();
                    Console.WriteLine($"已清除事件日志: {log.Log}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"无法清除事件日志: {log.Log}. 错误: {ex.Message}");
                }
            }

            // 获取并清除新式事件日志
            var wevtutilLogs = GetWevtutilLogs();
            foreach (var logName in wevtutilLogs)
            {
                try
                {
                    ClearModernEventLog(logName);
                    Console.WriteLine($"已清除事件日志: {logName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"无法清除事件日志: {logName}. 错误: {ex.Message}");
                }
            }

            Console.WriteLine("所有事件日志已清除完成。");
        }

        // 检查是否以管理员身份运行
        private static bool IsRunAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // 使用wevtutil获取新式事件日志列表
        private static string[] GetWevtutilLogs()
        {
            var process = new Process();
            process.StartInfo.FileName = "wevtutil";
            process.StartInfo.Arguments = "el";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception(process.StandardError.ReadToEnd());
            }

            return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        // 清除新式事件日志
        private static void ClearModernEventLog(string logName)
        {
            var process = new Process();
            process.StartInfo.FileName = "wevtutil";
            process.StartInfo.Arguments = $"cl {logName}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception(process.StandardError.ReadToEnd());
            }
        }
    }
}
