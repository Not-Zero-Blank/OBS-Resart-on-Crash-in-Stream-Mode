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
        static string OBS = SearchOBS();
        static void Main(string[] args)
        {
            Console.WriteLine($"Made by \"Mr. Blank#2709\" if you have issues contact me! ");
            dontclose();
            if (string.IsNullOrWhiteSpace(OBS))
            {
                OBS = FileDialog();
            }
            Console.WriteLine($"Found OBS in {OBS}");
            Environment.CurrentDirectory = Path.GetDirectoryName(OBS);
            Console.WriteLine("Starting OBS in Normal Mode");
            OBSNormalMode();
            Console.WriteLine("Press any Key in the Terminal to Exit");
        }
        static void OBSNormalMode()
        {
            try
            {
                var obs = Process.Start(OBS);
                obs.EnableRaisingEvents = true;
                obs.Exited += RestartOBS;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                OBSNormalMode();
            }
        }
        static void OBSStreamlMode()
        {
            try
            {
                var obs = Process.Start(OBS, "--startstreaming");
                obs.EnableRaisingEvents = true;
                obs.Exited += RestartOBS;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                OBSStreamlMode();
            }
        }
        static void dontclose()
        {
            new Thread(() => Console.ReadKey()).Start();
        }

        private static void RestartOBS(object sender, EventArgs e)
        {
            Console.WriteLine($"OBS Closed Restarting");
            Console.WriteLine("Starting OBS in Stream Mode");
            OBSStreamlMode();
        }

        static string SearchOBS()
        {
            if (!File.Exists(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\OBS Studio\OBS Studio (64bit).lnk"))
            {
                Console.WriteLine(@"Cant find OBS Studio in C:\ProgramData\Microsoft\Windows\Start Menu\Programs\OBS Studio    Please Select you own Path");
                Thread.Sleep(2000);
                return FileDialog();
            }
            return GetShortcutTarget(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\OBS Studio\OBS Studio (64bit).lnk");
        }
        static string FileDialog()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())    //skidded from Microsoft Docs :)
            {
                openFileDialog.InitialDirectory = @"C:\Program Files";
                openFileDialog.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "SELECT obs64.exe";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    return openFileDialog.FileName;
                }
                else
                {
                    return FileDialog();
                }
            }
        }
        private static string GetShortcutTarget(string file) // Skiddded from https://blez.wordpress.com/2013/02/18/get-file-shortcuts-target-with-c/
        {
            try
            {
                if (System.IO.Path.GetExtension(file).ToLower() != ".lnk")
                {
                    throw new Exception("Supplied file must be a .LNK file");
                }

                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream))
                {
                    fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                    uint flags = fileReader.ReadUInt32();        // Read flags
                    if ((flags & 1) == 1)
                    {                      // Bit 1 set means we have to
                                           // skip the shell item ID list
                        fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                        uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                        fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                    }

                    long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                                                                 // structure begins
                    uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                    fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                    uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                                                               // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                    fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                                                                                        // base pathname (target)
                    long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                                                                                                        // the base pathname. I don't need the 2 terminating nulls.
                    char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                    var link = new string(linkTarget);

                    int begin = link.IndexOf("\0\0");
                    if (begin > -1)
                    {
                        int end = link.IndexOf("\\\\", begin + 2) + 2;
                        end = link.IndexOf('\0', end) + 1;

                        string firstPart = link.Substring(0, begin);
                        string secondPart = link.Substring(end);

                        return firstPart + secondPart;
                    }
                    else
                    {
                        return link;
                    }
                }
            }
            catch
            {
                return "";
            }
        }
    }
}
