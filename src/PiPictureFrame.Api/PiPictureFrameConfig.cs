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

using System.Globalization;
using System.Xml.Linq;
using SethCS.Extensions;

namespace PiPictureFrame.Api
{
    public record PiPictureFrameConfig
    {
        // ---------------- Fields ----------------

        private static readonly string DefaultPhotoDirectory =
            Environment.GetFolderPath( Environment.SpecialFolder.MyPictures );

        // ---------------- Properties ----------------

        /// <summary>
        /// What time to put the frame to sleep.
        /// Null for never.
        /// </summary>
        public TimeOnly? SleepTime { get; init; } = null;

        /// <summary>
        /// What time to wake the frame up.
        /// Null for never.
        /// </summary>
        public TimeOnly? AwakeTime { get; init; } = null;

        /// <summary>
        /// The directory to search for photos.
        /// Directory should contains symlinks to other directories that contain
        /// photos.
        /// </summary>
        public string PhotoDirectory { get; init; } = DefaultPhotoDirectory;

        /// <summary>
        /// How often to check for photos on the disk.  0 or less
        /// for never.
        /// </summary>
        public TimeSpan PhotoRefreshInterval { get; init; } = TimeSpan.FromHours( 1 );

        /// <summary>
        /// How often to change the photo on screen.
        /// 0 or less for never.
        /// </summary>
        public TimeSpan PhotoChangeInterval { get; init; } = TimeSpan.FromMinutes( 1 );

        /// <summary>
        /// The brightness on a scale from 0-100.
        /// Anything more than 100 is truncated to 100.
        /// </summary>
        public byte Brightness { get; init; } = 75;
    }

    internal static class PiPictureFrameConfigExtensions
    {
        // ---------------- Fields ----------------

        private const string xmlElementName = "PiPictureFrameConfig";

        private const int xmlVersion = 1;

        // ---------------- Methods ----------------

        public static XDocument ToXml( this PiPictureFrameConfig config )
        {
            var dec = new XDeclaration( "1.0", "utf-8", "yes" );
            var doc = new XDocument( dec );

            var root = new XElement(
                xmlElementName,
                new XElement( "SleepTime", config.SleepTime?.ToString( "r", CultureInfo.InvariantCulture ) ?? "" ),
                new XElement( "AwakeTime", config.AwakeTime?.ToString( "r", CultureInfo.InvariantCulture ) ?? "" ),
                new XElement( "PhotoDirectory", config.PhotoDirectory ),
                new XElement( "PhotoRefreshInterval", config.PhotoRefreshInterval.TotalMinutes.ToString() ),
                new XElement( "PhotoChangeInterval", config.PhotoChangeInterval.TotalSeconds.ToString() ),
                new XElement( "Brightness", config.Brightness.ToString() )
            );
            doc.Add( root );
            root.Add(
                new XAttribute( "version", xmlVersion )
            );

            return doc;
        }

        public static PiPictureFrameConfig FromXml( XDocument doc )
        {
            var root = doc.Root;
            if( root is null )
            {
                throw new InvalidOperationException(
                    "Unable to parse meta data file.  Root is null"
                );
            }
            else if( xmlElementName.EqualsIgnoreCase( root.Name.LocalName ) == false )
            {
                throw new ArgumentException(
                    $"Invalid XML file, root node is '{root.Name.LocalName}', but expected '{xmlElementName}'."
                );
            }

            var config = new PiPictureFrameConfig();

            foreach( XElement child in root.Elements() )
            {
                string name = child.Name.LocalName;
                if( string.IsNullOrWhiteSpace( name ) )
                {
                    continue;
                }
                else if( name.EqualsIgnoreCase( "SleepTime" ) )
                {
                    if( string.IsNullOrEmpty( child.Value ) == false )
                    {
                        config = config with
                        {
                            SleepTime = TimeOnly.ParseExact(
                                child.Value,
                                "r",
                                CultureInfo.InvariantCulture
                            )
                        };
                    }
                }
                else if( name.EqualsIgnoreCase( "AwakeTime" ) )
                {
                    if( string.IsNullOrEmpty( child.Value ) == false )
                    {
                        config = config with
                        {
                            AwakeTime = TimeOnly.ParseExact(
                                child.Value,
                                "r",
                                CultureInfo.InvariantCulture
                            )
                        };
                    }
                }
                else if( name.EqualsIgnoreCase( "PhotoDirectory" ) )
                {
                    config = config with
                    {
                        PhotoDirectory = child.Value
                    };
                }
                else if( name.EqualsIgnoreCase( "PhotoRefreshInterval" ) )
                {
                    // Shouldn't be negative
                    double minutes = Math.Max( 0, double.Parse( child.Value ) );
                    config = config with
                    {
                        PhotoRefreshInterval = TimeSpan.FromMinutes( minutes )
                    };
                }
                else if( name.EqualsIgnoreCase( "PhotoChangeInterval" ) )
                {
                    // Shouldn't be negative.
                    double seconds = Math.Max( 0, double.Parse( child.Value ) );
                    config = config with
                    {
                        PhotoChangeInterval = TimeSpan.FromSeconds( seconds )
                    };
                }
                else if( name.EqualsIgnoreCase( "Brightness" ) )
                {
                    config = config with
                    {
                        Brightness = Math.Min( byte.Parse( child.Value ), (byte)100 )
                    };
                }
            }

            return config;
        }
    }
}
