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

using System.Xml.Linq;

namespace PiPictureFrame.Api
{
    public sealed class SettingsMgr
    {
        // ---------------- Events ----------------

        public event Action<PiPictureFrameConfig>? OnUpdatedSettings;

        // ---------------- Fields ----------------

        private static readonly FileInfo settingsFile = new FileInfo(
            Path.Combine( PiPictureFrameApi.AppDataDirectory.FullName, "Settings.xml" )
        );

        // ---------------- Constructor ----------------

        public SettingsMgr()
        {
            this.Settings = new PiPictureFrameConfig();
        }

        // ---------------- Properties ----------------

        public PiPictureFrameConfig Settings { get; private set; }

        // ---------------- Functions ----------------

        public void LoadSettings()
        {
            if( settingsFile.Exists )
            {
                XDocument doc = XDocument.Load( settingsFile.FullName );
                this.Settings = PiPictureFrameConfigExtensions.FromXml( doc );
            }
        }

        public void SaveSettings()
        {
            XDocument doc = this.Settings.ToXml();
            doc.Save( settingsFile.FullName );
        }

        public void UpdateSettings( PiPictureFrameConfig config )
        {
            ArgumentNullException.ThrowIfNull( config );

            this.Settings = config;
            SaveSettings();
            this.OnUpdatedSettings?.Invoke( this.Settings );
        }
    }
}
