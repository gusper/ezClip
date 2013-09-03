namespace Perez.Gus.Software.EzClip
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using System.Diagnostics;
   
    internal class App
    {
        /// <summary>
        /// Main entrypoint which handles processing command-line arguments.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Zero on success and other values represent failure.</returns>
        [STAThread]
        public static int Main(string[] args)
        {
            App app = new App();

            // Automatically copy input buffer to clipboard if one is present. 
            if (app.CopyInBufferIfAvailable())
            {
                return 0;
            }

            // If there was no data in the input buffer and no arguments were 
            // passed in, just display the help text and exit.
            if (args.Length == 0)
            {
                app.DisplayHelp();
                return 0;
            }

            // Process the command line arguments
            switch (args[0].ToLower())
            {
                case "/?":
                case "?":
                case "-?":
                case "/help":
                case "help":
                case "-help":
                    app.DisplayHelp();
                    break;

                case "paste":
                case "p":
                    app.DoPaste();
                    break;

                case "list":
                case "l":
                    app.DoList();
                    break;

                case "add":
                case "a":
                    {
                        string[] files = new string[args.Length - 1];
                        Array.Copy(args, 1, files, 0, files.Length);
                        app.DoCopy(files, true);
                    }

                    break;

                case "copy":
                case "c":
                    {
                        string[] files = new string[args.Length - 1];
                        Array.Copy(args, 1, files, 0, files.Length);
                        app.DoCopy(files, false);
                    }
                    
                    break;

                case "output":
                case "o":
                    {
                        StringBuilder cmdArgs = new StringBuilder();
                        
                        // Error out if no command/app was passed in to capture output from.
                        if (args.Length <= 1)
                        {
                            Console.WriteLine("The 'output' command requires providing something to execute (app or command).");
                            return 1;
                        }

                        // If arguments were provided (to pass to the app/cmd we're capturing 
                        // the output of) convert them to a string.
                        if (args.Length > 2)
                        {
                            for (int i = 2; i < args.Length; i++)
                            {
                                cmdArgs.Append(args[i] + " ");
                            }
                        }

                        app.DoOutput(args[1], cmdArgs.ToString());
                    }

                    break;

                default:
                    app.DisplayHelp();
                    break;
            }

            return 0;
        }

        /// <summary>
        /// Checks to see if there are characters in the input buffer. If there are, 
        /// we just copy it to the clipboard and exit. 
        /// </summary>
        /// <returns>
        /// True if there was data in the input buffer and it was 
        /// copied to the clipboard, false otherwise.
        /// </returns>
        private bool CopyInBufferIfAvailable()
        {
            // Ugly, hacky way I've ended up using to figure out if 
            // there's data in the input buffer or not.
            try
            {
                return Console.KeyAvailable;
            }
            catch (InvalidOperationException) 
            {
                // Copy the input buffer to the clipboard.
                string inputBuffer = Console.In.ReadToEnd();
                Clipboard.SetData(DataFormats.UnicodeText, inputBuffer);
            }

            return true;
        }

        /// <summary>
        /// Displays information about this tool as well as usage information
        /// and some basic help.
        /// </summary>
        private void DisplayHelp()
        {
            string msg =
                "ezClip - version 1.20\n" +
                "Written by Gus Perez - gus@gusperez.com - http://blog.gusperez.com\n" +
                "Provides clipboard copy/paste functionality from the command-line\n\n" +
                "usage: ezClip [copy|paste|add|list|output] <arguments>\n\n" +
                "For example:\n\n" +
                "  'ezClip copy somefile.txt myapp.exe'\n" +
                "   will copy somefile.txt and myapp.exe to the clipboard.\n\n" +
                "  'ezClip paste'\n" +
                "   will paste any files that are in the clipboard into the current directory.\n\n" +
                "  'ezClip list'\n" +
                "   will list the files that are currently on the clipboard\n\n" +
                "  'ezClip add somefile.cmd *.obj'\n" +
                "   will add somefile.cmd and all .obj files to the files already on the clipboard\n\n" +
                "  'ezClip output dir /b'\n" +
                "   will execute the command (3rd argument) using the provided parameters (4th argument+)\n" +
                "   and captures its output and then copies it to the clipboard. The example above will\n" +
                "   generate a list of files in the current directory and copies that list to the clipboard.\n\n" +
                "  '<command> | ezClip'\n" +
                "   will copy the piped in data stream to the clipboard. For example, 'dir | ezclip' will\n" +
                "   copy the output of the 'dir' command to the clipboard.\n\n" +
                "   Short forms also allowed for copy/paste/add/list/output by just using the corresponding\n" + 
                "   first letter. For example, 'ezclip c *.txt' will copy all text files to the clipboard.\n\n" +
                "For more information, bug reports, comments, or suggestions, please email gus@gusperez.com.\n\n" + 
                "Thanks go out to Joel McIntyre for contributing a few great enhancements as well.";

            Console.WriteLine(msg);
        }

        /// <summary>
        /// Gets all files currently in the clipboard and copies them into
        /// the current directory.  
        /// </summary>
        /// <returns>True if files were copied, false otherwise.</returns>
        private bool DoPaste()
        {
            // Get the data currently in the clipboard
            IDataObject dataObject = (IDataObject)Clipboard.GetDataObject();

            bool alwaysOverwrite = false;

            // Make sure that the clipboard contents are files.  If they
            // are, perform the copying of files
            if (dataObject.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);

                // Loop through all the filenames passed in and process them accordingly
                foreach (string f in files)
                {
                    FileInfo src = new FileInfo(f);
                    FileInfo dest = new FileInfo(Directory.GetCurrentDirectory() + "\\" + src.Name);

                    // If the file already exists, make sure the user is okay
                    // with overwriting it
                    if (dest.Exists && !alwaysOverwrite)
                    {
                        Console.Write("'{0}' already exists.  Overwrite? (y/n/a) ", dest.Name);
                        char c = Console.ReadKey().KeyChar;

                        if (Char.ToLower(c) != 'y' && Char.ToLower(c) != 'a')
                        {
                            continue;
                        }
                        else if (Char.ToLower(c) == 'a')
                        {
                            alwaysOverwrite = true;
                        }
                    }

                    try
                    {
                        File.Copy(f, dest.Name, true);
                    }
                    catch (System.IO.IOException ioe)
                    {
                        Console.WriteLine(ioe.Message);
                        Console.WriteLine("!!! error: Cannot overwrite '{0}'", dest.Name);
                        return false;
                    }
                }
            }
            else
            {
                Console.WriteLine("No files found in clipboard");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all files currently in the clipboard and copies them into
        /// the current directory.  
        /// </summary>
        /// <returns>True if files were listed, false otherwise (e.g., no files in clipboard)</returns>
        private bool DoList()
        {
            // Get the data currently in the clipboard
            IDataObject dataObject = (IDataObject)Clipboard.GetDataObject();

            // Make sure that the clipboard contents are files.  If they
            // are, perform the copying of files
            if (dataObject.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);

                // Loop through all the filenames passed in and process them accordingly
                foreach (string f in files)
                {
                    FileInfo src = new FileInfo(f);

                    Console.WriteLine(" " + src.FullName);
                }
            }
            else
            {
                Console.WriteLine("No files found in clipboard");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copies all the files specified on the command line to the clipboard.
        /// </summary>
        /// <param name="filelist">List of file names</param>
        /// <param name="addToExisting">If true, add to existing list in clipboard. If false, replace clipboard contents.</param>
        /// <returns>True if files were copied, false otherwise.</returns>
        private bool DoCopy(string[] filelist, bool addToExisting)
        {
            ArrayList validFiles = new ArrayList();

            // If no files were passed in, let the user know
            if (filelist.Length == 0)
            {
                Console.WriteLine("Filename(s) to copy were not specified");
                return false;
            }

            // Iterate through each file and verify they really exist
            foreach (string f in filelist)
            {
                if (f.Contains("*") || f.Contains("?"))
                {
                    foreach (string file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(f)), Path.GetFileName(f)))
                    {
                        validFiles.Add(file);
                    }
                }
                else
                {
                    FileInfo fi = new FileInfo(f);

                    // If file doesn't exist, show error, but continue processing
                    // remainder of the list
                    if (!fi.Exists)
                    {
                        Console.WriteLine("!!! error: '{0}' does not exist", f);
                        continue;
                    }

                    validFiles.Add(fi.FullName.ToString());
                }
            }

            // If none of the files were valid, we should error out now
            if (validFiles.Count == 0)
            {
                Console.WriteLine("No files were copied to the clipboard");
                return false;
            }

            if (addToExisting)
            {
                // Get the data currently in the clipboard
                IDataObject dataObj = (IDataObject)Clipboard.GetDataObject();

                // Make sure that the clipboard contents are files.  If they
                // are, perform the copying of files
                if (dataObj.GetDataPresent(DataFormats.FileDrop, false))
                {
                    string[] existingFiles = (string[])dataObj.GetData(DataFormats.FileDrop);

                    foreach (string file in existingFiles)
                    {
                        validFiles.Add(file);
                    }
                }
            }

            // Copy the list of files to the clipboard
            IDataObject dataObject = new DataObject(DataFormats.FileDrop, (string[])validFiles.ToArray(typeof(string)));
            Clipboard.SetDataObject(dataObject, true);

            return true;
        }

        /// <summary>
        /// Runs the command passed in, captures its output, and 
        /// copies the captured output to the clipboard.
        /// </summary>
        /// <param name="cmd">Application/command to launch and capture output from</param>
        /// <param name="args">Arguments to pass to app/command</param>
        /// <returns>True if output was captured, false otherwise.</returns>
        private bool DoOutput(string cmd, string args)
        {
            // Set up the process to redirect and capture output.
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c" + cmd + " " + args);
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;

            try
            {
                // Start the process and capture its output.
                Process proc = Process.Start(psi);
                string cmdOutput = proc.StandardOutput.ReadToEnd();

                // Copy the output text to the clipboard.
                Clipboard.SetData(DataFormats.UnicodeText, cmdOutput);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong, please copy and paste the following and send to gus@gusperez.com:");
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                return false;
            }

            return true;
        }
    }
}
