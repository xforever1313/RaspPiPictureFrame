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

using Cake.Common.Tools.MSBuild;
using Cake.Frosting;
using Seth.CakeLib.DebPacker;

namespace DevOps.DebPack
{
    [TaskName( "arm32_deb_pack" )]
    public sealed class Arm32DebPack : DebPackTask
    {
        // ---------------- Properties ----------------

        public override PlatformTarget Architecture => PlatformTarget.ARM;
    }

    [TaskName( "arm64_deb_pack" )]
    public sealed class Arm64DebPack : DebPackTask
    {
        // ---------------- Properties ----------------

        public override PlatformTarget Architecture => PlatformTarget.ARM64;
    }

    [TaskName( "x64_deb_pack" )]
    public sealed class X64DebPack : DebPackTask
    {
        // ---------------- Properties ----------------

        public override PlatformTarget Architecture => PlatformTarget.x64;
    }

    public abstract class DebPackTask : DevopsTask
    {
        // ---------------- Properties ----------------

        public abstract PlatformTarget Architecture { get; }

        // ---------------- Functions ----------------

        public override bool ShouldRun( BuildContext context )
        {
            return DebPackerRunner.CanRun( context );
        }

        public override void Run( BuildContext context )
        {
            var debPackConfig = new DebPackConfig( context, this.Architecture );

            var runner = new DebPackerRunner( context );
            runner.DebianPack( debPackConfig );
        }
    }
}
