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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiPictureFrame.Api
{
    public sealed class FileManager
    {
        // ---------------- Fields ----------------

        private readonly DirectoryInfo pictureDirectory;

        // ---------------- Constructor ----------------

        public FileManager( DirectoryInfo pictureDirectory )
        {
            this.pictureDirectory = pictureDirectory;
        }

        // ---------------- Functions ----------------

        public IEnumerable<string> GetSubDirectoryNames()
        {
            var dirNames = new List<string>();

            foreach( DirectoryInfo subDir in this.pictureDirectory.GetDirectories() )
            {
                dirNames.Add( subDir.Name );
            }

            return dirNames;
        }
    }
}
