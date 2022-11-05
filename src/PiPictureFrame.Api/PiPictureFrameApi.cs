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

using PiPictureFrame.Api.Renders;
using PiPictureFrame.Api.Screens;
using Serilog;

namespace PiPictureFrame.Api
{
    public interface IPiPictureFrameApi
    {
        // ---------------- Properties ----------------

        Version ApiVersion { get; }

        FileManager FileManager { get; }

        IRenderer Renderer { get; }

        IScreen Screen { get; }

        SettingsMgr Settings { get; }

        SystemController System { get; }
    }

    public sealed class PiPictureFrameApi : IPiPictureFrameApi, IDisposable
    {
        // ---------------- Fields ----------------

        public static readonly DirectoryInfo AppDataDirectory =
            new DirectoryInfo(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData ),
                    "RaspPiPictureFrame"
                )
            );

        private readonly TaskScheduler taskScheduler;

        private readonly PiPictureFrameApiConfig apiConfig;

        private readonly ILogger log;

        // ---------------- Constructor ----------------

        public PiPictureFrameApi( PiPictureFrameApiConfig config, ILogger log )
        {
            this.apiConfig = config;
            this.log = log;

            this.ApiVersion = typeof( PiPictureFrameApi ).Assembly.GetName().Version ?? new Version( 0, 0, 0 );
            this.FileManager = new FileManager( this.apiConfig.PictureDirectory );
            this.Settings = new SettingsMgr();
            this.Screen = new PiTouchScreen( this.apiConfig, this.log );
            this.Renderer = new PqivRenderer( this.log );
            this.System = new SystemController( this.log );

            this.taskScheduler = new TaskScheduler( this, this.log );
        }

        static PiPictureFrameApi()
        {
            Resources = new Resources();
        }

        // ---------------- Properties ----------------

        public static Resources Resources { get; private set; }

        public Version ApiVersion { get; private set; }

        public FileManager FileManager { get; private set; }

        public IRenderer Renderer { get; private set; }

        public IScreen Screen { get; private set; }

        public SettingsMgr Settings { get; private set; }

        public SystemController System { get; private set; }

        // ---------------- Functions ----------------

        public void Init()
        {
            // CreateDirectory already no-ops of the directory exists.
            Directory.CreateDirectory( AppDataDirectory.FullName );

            this.Settings.LoadSettings();
            this.log.Information( "User Settings Loaded." );

            this.Screen.Refresh();
            this.Screen.SetBrightness( this.Settings.Settings.Brightness );
            this.log.Information( "Refreshed Screen Settings." );

            this.Renderer.Init( this.apiConfig.PictureDirectory );
            this.log.Information( "Renderer Started." );

            this.taskScheduler.UpdateTasks( this.Settings.Settings );
            this.Settings.OnUpdatedSettings += Settings_OnUpdatedSettings;
            this.taskScheduler.Start();
            this.log.Information( "Task Scheduler Started." );
        }

        public void Dispose()
        {
            this.log.Information( "Stopping Task Scheduler." );
            this.Settings.OnUpdatedSettings -= Settings_OnUpdatedSettings;
            this.taskScheduler.Dispose();

            this.log.Information( "Stopping System Controller." );
            IDisposable system = this.System;
            system.Dispose();

            this.log.Information( "Stopping Renderer." );
            this.Renderer.Dispose();

            this.log.Information( "Stopping Screen." );
            this.Screen.Dispose();

            this.log.Information( "Usr Settings Saved." );
            this.Settings.SaveSettings();
        }

        private void Settings_OnUpdatedSettings( PiPictureFrameConfig newSettings )
        {
            this.taskScheduler.UpdateTasks( newSettings );
            this.Screen.SetBrightness( newSettings.Brightness );
        }
    }
}
