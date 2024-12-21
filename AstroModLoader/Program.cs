﻿using CommandLine;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AstroModLoader
{
    public class Options
    {
        [Option("server", Required = false, HelpText = "Specifies that AstroModLoader is being ran for a server.")]
        public bool ServerMode { get; set; }

        [Option("client", Required = false, HelpText = "Specifies that AstroModLoader is being ran for a client.")]
        public bool ForceClient { get; set; }

        [Option("data", Required = false, HelpText = "Specifies the %localappdata% folder or the local equivalent of it.")]
        public string LocalDataPath { get; set; }

        [Option("next_launch_path", Required = false, HelpText = "Specifies a path to a file to store as the launch script.")]
        public string NextLaunchPath { get; set; }

        [Option("install_mod", Required = false, HelpText = "Specifies a path to a mod to install.")]
        public string InstallMod { get; set; }

        [Option("install_thunderstore", Required = false, HelpText = "Used for the ror2mm URL protocol.")]
        public string InstallThunderstore { get; set; }
    }

    public static class Program
    {
        public static Options CommandLineOptions;

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                CommandLineOptions = o;
                if (CommandLineOptions.ForceClient) CommandLineOptions.ServerMode = false;
                else if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "AstroServer.exe"))) CommandLineOptions.ServerMode = true;

                if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetDefaultFont(new Font(new FontFamily("Microsoft Sans Serif"), 8.25f)); // default font changed in .NET Core 3.0

                // if available, we want to accept the ror2mm url protocol; but if other software is installed that accepts it, we want them to override us
                try
                {
                    string thunderstoreProtocol = "ror2mm";
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + thunderstoreProtocol);
                    bool canWeContinue = key == null;
                    if (key != null)
                    {
                        var key2 = key.OpenSubKey(@"shell\open\command");
                        if (key2.GetValue(string.Empty) is string blah && blah.Contains("AstroModLoader"))
                        {
                            canWeContinue = true;
                        }
                        key2.Close();
                        key.Close();
                    }
                    if (canWeContinue)
                    {
                        key = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + thunderstoreProtocol);
                        key.SetValue(string.Empty, "URL: " + thunderstoreProtocol);
                        key.SetValue("URL Protocol", string.Empty);

                        var key2 = key.CreateSubKey(@"shell\open\command");
                        key2.SetValue(string.Empty, Application.ExecutablePath + " --install_thunderstore=" + "%1");
                        key2.Close();
                        key.Close();
                    }
                }
                catch
                {
                    // no big deal if it doesn't work
                }

                Form1 f1 = new Form1();
                Application.Run(f1);
            });
        }
    }
}
