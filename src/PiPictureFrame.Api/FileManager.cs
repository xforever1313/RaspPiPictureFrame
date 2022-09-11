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

        public void UploadPictureToDirectory(
            string directoryName,
            Stream fileStream,
            string fileName
        )
        {
            string fullPath = Path.Combine( this.pictureDirectory.FullName, directoryName );
            DirectoryInfo uploadDirectory = new DirectoryInfo( fullPath );
            if( uploadDirectory.Exists == false )
            {
                Directory.CreateDirectory( uploadDirectory.FullName );
            }

            fullPath = Path.Combine( fullPath, fileName );
            if( File.Exists( fullPath ) )
            {
                throw new ArgumentException(
                    $"File '{fileName}' already exists in {directoryName}",
                    nameof( fileName )
                );
            }

            using( var ostream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fileStream.CopyTo( ostream );
            }
        }
    }
}
