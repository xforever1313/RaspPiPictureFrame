﻿//
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
using PiPictureFrame.Api.Renders;

namespace PiPictureFrame.Api
{
    public sealed class PiPictureFrameApi : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly PiPictureFrameApiConfig apiConfig;

        private readonly ILogger log;

        // ---------------- Constructor ----------------

        public PiPictureFrameApi( PiPictureFrameApiConfig config, ILogger log )
        {
            this.apiConfig = config;
            this.log = log;

            this.ApiVersion = typeof( PiPictureFrameApi ).Assembly.GetName().Version ?? new Version( 0, 0, 0 );
            this.Settings = new SettingsMgr();
            this.Renderer = new PqivRenderer( log );
        }

        static PiPictureFrameApi()
        {
            Resources = new Resources();
        }

        // ---------------- Properties ----------------

        public Version ApiVersion { get; private set; }

        public IRenderer Renderer { get; private set; }

        public static Resources Resources { get; private set; }

        public SettingsMgr Settings { get; private set; }

        // ---------------- Functions ----------------

        public void Init()
        {
            this.Settings.LoadSettings();
            this.log.LogInformation( "User Settings Loaded." );

            this.Renderer.Init( this.apiConfig.PictureDirectory );
            this.log.LogInformation( "Renderer Started." );
        }

        public void Dispose()
        {
            this.log.LogInformation( "Stopping Renderer." );
            this.Renderer.Dispose();

            this.log.LogInformation( "Usr Settings Saved." );
            this.Settings.SaveSettings();
        }
    }
}
