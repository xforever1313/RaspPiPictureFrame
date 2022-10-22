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

using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Publish;
using Cake.Common.Tools.MSBuild;
using Cake.Core.IO;
using Seth.CakeLib;
using Seth.CakeLib.DebPacker;

namespace DevOps.DebPack
{
    internal sealed class DebPackConfig : DebPackageConfig
    {
        // ---------------- Fields ----------------

        private readonly BuildContext context;

        private readonly PlatformTarget target;

        private readonly Version version;

        // ---------------- Constructor ----------------

        public DebPackConfig( BuildContext context, PlatformTarget target )
        {
            this.context = context;
            this.target = target;
            this.version = ParseVersion( context.WebCsProj );
        }

        // ---------------- Properties ----------------

        public override PlatformTarget Architecture => this.target;

        public override string PackageName => "RaspPiPictureFrame";

        public override string Maintainer =>
            "Seth Hendrick";

        public override string Description =>
            "A digital picture frame for Linux systems.";

        public override string Homepage =>
            "https://github.com/xforever1313/RaspPiPictureFrame";

        public override uint PackageRevision => 0;

        public override Version PackageVersion => this.version;

        /// <summary>
        /// Should work on any OS.
        /// </summary>
        public override string TargetOperatingSystem => "";

        public override DirectoryPath WorkingDirectory =>
            this.context.DistDirectory.Combine( this.target.ToDotnetRid( PlatformID.Unix ) );

        // ---------------- Functions ----------------

        public override void MoveFilesIntoPackage( DirectoryPath packageRoot )
        {
            DirectoryPath usrFolder = packageRoot.Combine( "usr" );
            DirectoryPath libFolder = usrFolder.Combine( "lib" );
            DirectoryPath installDirectory = libFolder.Combine( "RaspPiPictureFrame" );

            var publishSettings = new DotNetCorePublishSettings
            {
                Configuration = "Release",
                OutputDirectory = installDirectory,
                PublishReadyToRun = true,
                Runtime = this.Architecture.ToDotnetRid( PlatformID.Unix ),
                SelfContained = true // <- So we don't have to worry about runtime being installed.
            };

            this.context.Information( "Publishing Dotnet Package" );
            this.context.DotNetCorePublish(
                this.context.WebCsProj.ToString(),
                publishSettings
            );

            AddUdevRules( packageRoot );
        }

        /// <summary>
        /// Add UDEV rules so we can turn on and off the backlight, and adjust the brightness.
        /// 
        /// Taken from here: https://forums.raspberrypi.com/viewtopic.php?t=136749
        /// </summary>
        /// <param name="packageRoot"></param>
        private void AddUdevRules( DirectoryPath packageRoot )
        {
            DirectoryPath udevFolder = packageRoot.Combine( "etc/udev/rules.d/" );
            this.context.EnsureDirectoryExists( udevFolder );
            this.context.CleanDirectory( udevFolder );

            const string fileContents = "SUBSYSTEM==\"backlight\",RUN+=\"/bin/chmod 666 /sys/class/backlight/%k/brightness /sys/class/backlight/%k/bl_power\"";

            FilePath backLightRulesFile = udevFolder.CombineWithFilePath( "backlight-permissions.rules" );
            File.WriteAllText(
                backLightRulesFile.FullPath,
                fileContents
            );

        }

        private static Version ParseVersion( FilePath csProj )
        {
            using( var fStream = new FileStream( csProj.ToString(), FileMode.Open, FileAccess.Read ) )
            {
                XDocument doc = XDocument.Load( fStream );
                XElement? root = doc.Root;
                if( root is null )
                {
                    throw new InvalidOperationException(
                        $"Unable to get root element from {csProj}"
                    );
                }

                foreach( XElement element in root.Elements() )
                {
                    if( "PropertyGroup" == element.Name.LocalName )
                    {
                        foreach( XElement propertyGroup in element.Elements())
                        {
                            if( "Version" == propertyGroup.Name.LocalName )
                            {
                                return Version.Parse( propertyGroup.Value );
                            }
                        }
                    }
                }

                throw new InvalidOperationException( $"Could not get version from {csProj}" );
            }
        }
    }
}
