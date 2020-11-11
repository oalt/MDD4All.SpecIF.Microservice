using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MDD4All.SpecIF.Apps.ServiceStarter.Controllers
{
    public class ServiceProcessController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Process StartProcess(string executable,
                                           string parameters,
                                           string workDirectoy,
                                           ProcessWindowStyle windowStyle,
                                           EventHandler eventHandler,
                                           bool useShellExecute = false,
                                           bool createNoWindow = false)
        {
            Process process = new Process();

            // Configure the process using the StartInfo properties.
            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = parameters;
            process.StartInfo.WindowStyle = windowStyle;
            process.StartInfo.CreateNoWindow = createNoWindow;
            process.StartInfo.UseShellExecute = useShellExecute;
            process.StartInfo.WorkingDirectory = workDirectoy;

            process.EnableRaisingEvents = true;

            process.Exited += eventHandler;

            process.Start();

            logger.Info("Starting process: " + process.ProcessName + " [" + process.Id + "]");

            return process;
        }
    }
}
