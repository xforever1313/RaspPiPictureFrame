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

using Microsoft.Extensions.Logging;

namespace PiPictureFrame.Api.Screens
{
    internal sealed class PiTouchScreen : IScreen
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// File to control brightness.
        /// </summary>
        private readonly FileInfo brightnessFile;

        /// <summary>
        /// File to turn on/off screen.
        /// </summary>
        private readonly FileInfo powerFile;

        private readonly ILogger log;

        private readonly object brightnessFileLock;
        private readonly object powerFileLock;

        private readonly bool isLinux;

        // ---------------- Constructor ----------------

        public PiTouchScreen( PiPictureFrameApiConfig config, ILogger log )
        {
            this.brightnessFile = config.RpiBacklightBrightnessFile;
            this.powerFile = config.RpiBacklightPowerFile;
            this.log = log;
            this.isLinux = ( Environment.OSVersion.Platform == PlatformID.Unix );

            this.brightnessFileLock = new object();
            this.powerFileLock = new object();
        }

        // ---------------- Properties ----------------

        public byte Brightness { get; private set; }

        public bool IsOn { get; private set; }

        // ---------------- Functions ----------------

        void IDisposable.Dispose()
        {
            // Nothing to do.
        }

        public void Refresh()
        {
            if( this.isLinux == false )
            {
                this.log.LogWarning(
                    $"{nameof( PiTouchScreen )} only compatible on Linux systems, can not refresh."
                );
                return;
            }

            this.RefreshIsOn();
            this.RefreshBrightness();
        }

        private void RefreshIsOn()
        {
            if( this.powerFile.Exists == false )
            {
                this.log.LogWarning(
                    $"{nameof( PiTouchScreen )} - Missing file '{powerFile.FullName}', can not refresh."
                );
                return;
            }

            lock( this.powerFileLock )
            {
                string isOnString = this.ReadFile( powerFile );
                if( string.IsNullOrWhiteSpace( isOnString ) == false )
                {
                    if( isOnString.StartsWith( "0" ) )
                    {
                        this.IsOn = true;
                    }
                    else
                    {
                        this.IsOn = false;
                    }
                }
            }
        }

        private void RefreshBrightness()
        {
            if( brightnessFile.Exists == false )
            {
                this.log.LogWarning(
                    $"{nameof( PiTouchScreen )} - Missing file '{brightnessFile.FullName}', can not refresh."
                );
                return;
            }

            lock( this.brightnessFileLock )
            {
                string isOnString = this.ReadFile( brightnessFile );
                if( string.IsNullOrWhiteSpace( isOnString ) == false )
                {
                    int brightness;
                    if( int.TryParse( isOnString, out brightness ) && ( brightness > 0 ) )
                    {
                        // Let 20 be 0, 255 be 100.
                        // Normalized taken from here:
                        // https://docs.tibco.com/pub/spotfire/7.0.1/doc/html/norm/norm_scale_between_0_and_1.htm
                        double normalized = ( brightness - 20.0 ) / ( 255.0 - 20 ) * 100;
                        this.Brightness = (byte)Math.Ceiling( normalized );
                    }
                }
            }
        }

        public void SetBrightness( byte newValue )
        {
            if( newValue == this.Brightness )
            {
                return;
            }
            else if( this.isLinux == false )
            {
                this.log.LogWarning(
                    $"{nameof( PiTouchScreen )} only compatible on Linux systems, can not set brightness."
                );
                return;
            }
            else if( brightnessFile.Exists == false )
            {
                this.log.LogWarning(
                    $"'{brightnessFile.FullName}' does not exist, can not set brightness."
                );
                return;
            }

            if( newValue > 100 )
            {
                newValue = 100;
            }

            int brightnessNormalized = (int)Math.Floor( ( newValue ) / ( 100.0 ) * 255.0 );

            lock( this.brightnessFileLock )
            {
                WriteFile( brightnessFile, brightnessNormalized.ToString() );
            }

            this.Brightness = newValue;
        }

        public void SetOn( bool newValue )
        {
            if( newValue == this.IsOn )
            {
                return;
            }
            else if( this.isLinux == false )
            {
                this.log.LogWarning(
                    $"{nameof( PiTouchScreen )} only compatible on Linux systems, can not turn on or off."
                );
                return;
            }
            else if( this.powerFile.Exists == false )
            {
                this.log.LogWarning(
                    $"'{powerFile.FullName}' does not exist, can not turn screen on or off.."
                );
                return;
            }

            // 0 to turn on screen, else 1.
            string s = newValue ? "0" : "1";

            lock( this.powerFileLock )
            {
                WriteFile( powerFile, s );
            }

            this.IsOn = newValue;
        }

        private void WriteFile( FileInfo filePath, string value )
        {
            this.log.LogInformation( $"Writing '{value}' to: {filePath.FullName}" );

            File.WriteAllText( filePath.FullName, value );
        }

        private string ReadFile( FileInfo filePath )
        {
            string fileContents = File.ReadAllText( filePath.FullName ).Trim();
            this.log.LogInformation( $"Read '{fileContents}' from: {filePath.FullName}" );

            return fileContents;
        }
    }
}
