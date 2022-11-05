//
// PiPictureFrame - Digital Picture Frame built for the Raspberry Pi.
// Copyright (C) 2022 Seth Hendrick
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System.Diagnostics;
using System.Text.RegularExpressions;
using Serilog;

namespace PiPictureFrame.Api.Renders
{
    internal sealed class PqivRenderer : IRenderer
    {
        // ---------------- Fields ----------------

        private readonly bool isLinux;

        private const string defaultPqivLocation = "/usr/bin/pqiv";

        private readonly FileInfo pqivExe;

        private readonly ILogger log;

        private Process? pqivProcess;

        private FileInfo? currentPicture;
        private readonly object currentPictureLock;
        private static readonly Regex currentPictureRegex = new Regex(
            @"CURRENT_FILE_NAME=""(?<fileName>.+)""",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        private static readonly Regex currentFileIndexRegex = new Regex(
            @"CURRENT_FILE_INDEX=(?<index>\d+)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        // ---------------- Constructor ----------------

        public PqivRenderer( ILogger log ) :
            this( log, new FileInfo( Environment.GetEnvironmentVariable( "PQIV_EXE_PATH" ) ?? defaultPqivLocation ) )
        {
        }

        internal PqivRenderer( ILogger log, FileInfo pqivExe )
        {
            this.isLinux = ( Environment.OSVersion.Platform == PlatformID.Unix );
            this.log = log;
            this.pqivExe = pqivExe;

            this.pqivProcess = null;

            this.currentPictureLock = new object();
            this.currentPicture = null;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Returns the current picture path, which for pqiv is always
        /// ./.pqiv-select.
        /// </summary>
        public FileInfo? CurrentPicturePath
        {
            get
            {
                lock( this.currentPictureLock )
                {
                    return this.currentPicture;
                }
            }
            private set
            {
                lock( this.currentPictureLock )
                {
                    this.currentPicture = value;
                }
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Inits and starts pqiv.
        /// </summary>
        public void Init( DirectoryInfo pictureDirectory )
        {
            if( this.isLinux == false )
            {
                this.log.Warning(
                    "PQIV can only be run on Linux, nothing will be rendered"
                );
                return;
            }

            FindExecutable();

            ProcessStartInfo info = new ProcessStartInfo();

            // Arguments (Per man page):
            // -f, --fullscreen
            //       Start in fullscreen mode. Fullscreen can be toggled by pressing f at runtime by default.
            // -i, --hide-info-box
            //        Initially hide the info box. Whether the box is visible can be toggled by pressing i at runtime by default.
            // -F, --fade
            //        Fade between images. See also --fade-duration.
            // --end-of-files-action=ACTION
            //        If  all  files  have  been  viewed  and  the next image is to be viewed, either by the user's request or because a
            //        slideshow is active, pqiv by default cycles and restarts at the first image. This parameter can be used to  modify
            //        this behaviour. Valid choices for ACTION are:
            // 
            //        quit                Quit pqiv,
            // 
            //        wait                Wait  until  a  new  image  becomes  available.  This  only  makes  sense  if  used  with e.g.
            //                            --watch-directories,
            // 
            //        wrap (default)      Restart at the first image. In shuffle mode, choose a new random order,
            // 
            //        wrap-no-reshuffle   As wrap, but do not reshuffle in random mode.
            // --shuffle
            //        Display  files in random order. This option conflicts with --sort. Files are reshuffled after all images have been
            //        shown, but within one cycle, the order is stable. The reshuffling can be disabled using --end-of-files-action.  At
            //        runtime, you can use Control + R by default to toggle shuffle mode; this retains the shuffled order, i.e., you can
            //        disable shuffle mode, view a few images, then enable it again and continue after the last image you viewed earlier
            // --watch-directories
            //        Watch all directories supplied as parameters to pqiv for new files and add them as they appear.  In  --sort  mode,
            //        files  are  sorted  into  the  correct  position,  else,  they  are  appended  to  the  end of the list.  See also
            //        --watch-files, which handles how changes to the image that is currently being viewed are handled.
            //
            // --actions-from-stdin
            //        Like --action, but read actions from the standard input. See the ACTIONS section below for  syntax  and  available
            //        commands. This option conflicts with --additional-from-stdin.
            //        in shuffle mode.

            // Disable libav (ffmpeg) since it seems to cause a lockup with pqiv.
            info.Arguments = "--disable-backends=libav --fullscreen --hide-info-box --fade --scale-images-up --end-of-files-action=wrap --shuffle --watch-directories --actions-from-stdin \"" + pictureDirectory.FullName + "\"";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.FileName = defaultPqivLocation;

            this.log.Information( "Starting Pqiv with arguments: " + info.Arguments );

            this.pqivProcess = new Process();
            this.pqivProcess.StartInfo = info;
            this.pqivProcess.OutputDataReceived += this.StdoutProcessor;
            this.pqivProcess.ErrorDataReceived += this.StderrProcessor;
            this.pqivProcess.Start();
            this.pqivProcess.BeginOutputReadLine();
            this.pqivProcess.BeginErrorReadLine();

            // So we can see what the current picture is.
            this.pqivProcess.StandardInput.WriteLine( "set_status_output(1)" );
        }

        /// <summary>
        /// Disposes this class.
        /// Makes sure all child processes are killed.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.log.Information( "Quitting Pqiv..." );

            this.pqivProcess?.StandardInput.WriteLine( "quit()" );
            bool exited = this.pqivProcess?.WaitForExit( 5000 ) ?? true;
            if( exited == false )
            {
                this.log.Warning( "PQIV took too long to exit, killing process." );
                this.pqivProcess?.Kill();
            }

            this.pqivProcess?.Dispose();
            this.log.Information( "Quitting Pqiv...Done!" );
        }

        public void GoToNextPicture()
        {
            if( this.isLinux == false )
            {
                this.log.Warning( "Windows Machine, can not run PQIV and render picture." );
                return;
            }
            
            if( pqivProcess is null )
            {
                throw new InvalidOperationException( nameof( this.Init ) + "() must be called first!" );
            }
            else
            {
                // Tell pqiv to go to the next file.
                this.pqivProcess.StandardInput.WriteLine( "goto_file_relative(1)" );
            }
        }

        private void FindExecutable()
        {
            ProcessStartInfo info = new ProcessStartInfo( this.pqivExe.FullName, "--help" );
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;

            using( Process process = new Process() )
            {
                process.StartInfo = info;
                if( process.Start() == false )
                {
                    throw new ApplicationException( "Could not start " + this.pqivExe.FullName );
                }

                process.WaitForExit();
                int exitCode = process.ExitCode;
                if( exitCode != 0 )
                {
                    throw new ApplicationException(
                        $"Trying to execute '{this.pqivExe.FullName} --help' failed."
                    );
                }
            }
        }

        private void StdoutProcessor( object sender, DataReceivedEventArgs e )
        {
            // Per MSDN, when the process is exiting, a null line is sent.
            // we need to account for that.
            //
            // Per this stack overflow page: http://stackoverflow.com/questions/11631443/capturing-process-output-via-outputdatareceived-event
            // the child process needs to flush stdout in order for this to read it, otherwise
            // it will never get stdout until it exits.
            //
            // In pqiv.c, line 2589 needs fflush(stdout) in order for this to work properly.
            // Easiest way to find that is to serach for "CURRENT_FILE_NAME=" in the file, and add fflush(stdout); after the printf.
            if( ( e is not null ) && ( string.IsNullOrEmpty( e.Data ) == false ) )
            {
                string line = e.Data;

                Match match = currentPictureRegex.Match( line );
                if( match.Success )
                {
                    this.log.Debug( "PQIV: " + line );
                    this.CurrentPicturePath = new FileInfo( match.Groups["fileName"].Value );
                }
                else if ( currentFileIndexRegex.IsMatch( line ) )
                {
                    this.log.Debug( "PQIV: " + line );
                }
                else
                {
                    this.log.Warning( "PQIV: " + line );
                }
            }
        }

        private void StderrProcessor( object sender, DataReceivedEventArgs e )
        {
            if( ( e is not null ) && ( string.IsNullOrEmpty( e.Data ) == false ) )
            {
                this.log.Warning( "PQIV STDERR: " + e.Data );
            }
        }
    }
}
