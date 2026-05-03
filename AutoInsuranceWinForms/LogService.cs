using System;
using System.IO;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public static class LogService
    {
        private static readonly string LogDirectory = Path.Combine(Application.StartupPath, "logs");
        private static readonly string LogFile = Path.Combine(LogDirectory, "activity.log");

        public static void Log(string action, string details)
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(LogFile,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + action + " | " + details + Environment.NewLine);
        }
    }
}
