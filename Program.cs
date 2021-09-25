using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OBS_Restream_on_Crash_by_Blank_Windows_Edition
{
    class Program
    {
        static string OBS = string.Empty;
        static Process OBSProcess = null;
        static void Main(string[] args)
        {
            Console.Title = "OBS Restart on Crash";
            Console.CursorVisible = false;
            Console.WriteLine($"Made by \"Mr. Blank#2709\" if you have issues contact me! ");
            dontclose();
            AttachProcesstoOBs();
            Console.WriteLine("Press Enter in the Terminal to Exit");
        }
        static bool IsObs(string File)
        {
            var test = FileVersionInfo.GetVersionInfo(File).FileDescription;
            if (test == "OBS Studio")
            {
                return true;
            }
            return false;
        }
        static Process GetOBS()
        {
            foreach (Process a in Process.GetProcesses())
            {
                try
                {
                    if (IsObs(a.MainModule.FileName))
                    {
                        return a;
                    }
                }
                catch
                { }
            }
            return null;
        }
        static void AttachProcesstoOBs()
        {
            try
            {
                OBSProcess = GetOBS();
                if (OBSProcess == null)
                {
                    Console.WriteLine("Await OBS Start\nPlease Start OBS");
                    while (GetOBS() == null)
                    {

                    }
                    OBSProcess = GetOBS();
                }
                Console.WriteLine($"Found OBS with Process ID {OBSProcess.Id}");               
                OBS = OBSProcess.MainModule.FileName;
                Console.WriteLine($"Found OBS in {OBS}");
                Environment.CurrentDirectory = Path.GetDirectoryName(OBS);
                Console.WriteLine("Set Enviroment for OBS");
                OBSProcess.EnableRaisingEvents = true;
                OBSProcess.Exited += RestartOBS;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                AttachProcesstoOBs();
            }
        }
        static void OBSStreamlMode()
        {
            try
            {
                OBSProcess = Process.Start(OBS, "--startstreaming");
                OBSProcess.EnableRaisingEvents = true;
                OBSProcess.Exited += RestartOBS;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                OBSStreamlMode();
            }
        }
        static void dontclose()
        {
            new Thread(() => Console.ReadLine()).Start();
        }

        private static void RestartOBS(object sender, EventArgs e)
        {
            Console.WriteLine($"OBS Closed ({OBSProcess.ExitTime}) Restarting");
            Console.WriteLine("Starting OBS in Stream Mode");
            OBSStreamlMode();
        }
    }
}
