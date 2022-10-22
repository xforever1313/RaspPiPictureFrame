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
    public record PiPictureFrameApiConfig
    {
        // ---------------- Fields ----------------

        public static readonly DirectoryInfo DefaultPhotoDirectory =
            new DirectoryInfo(
                Environment.GetFolderPath( Environment.SpecialFolder.MyPictures )
            );

        /// <remarks>
        /// If, instead of "rpi_backlight"
        /// it is "10-0045", you may need to configure config.txt
        /// file and comment out dtoverlay=vc4-kms-v3d
        /// and max_framebuffer=2.
        /// </remarks>
        public static readonly FileInfo DefaultRpiBacklightPowerFile =
            new FileInfo(
                "/sys/class/backlight/rpi_backlight/bl_power"
            );

        /// <remarks>
        /// If, instead of "rpi_backlight"
        /// it is "10-0045", you may need to configure config.txt
        /// file and comment out dtoverlay=vc4-kms-v3d
        /// and max_framebuffer=2.
        /// </remarks>
        public static readonly FileInfo DefaultRpiBacklightBrightnessFile =
            new FileInfo(
                "/sys/class/backlight/rpi_backlight/brightness"
            );

        // ---------------- Properties ----------------

        public DirectoryInfo PictureDirectory { get; init; } = DefaultPhotoDirectory;

        /// <summary>
        /// When using the Raspberry PI Backlight touchscreen,
        /// this is the file the controls if is on or off.
        /// </summary>
        public FileInfo RpiBacklightPowerFile { get; init; } = DefaultRpiBacklightPowerFile;

        /// <summary>
        /// When using the Raspberry Pi Backlight touchscreen,
        /// this is the file that controls the brightness of the screen.
        /// </summary>
        public FileInfo RpiBacklightBrightnessFile { get; init; } = DefaultRpiBacklightBrightnessFile;
    }
}
