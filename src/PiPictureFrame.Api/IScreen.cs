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
    /// <summary>
    /// Represents a screen to render images on.
    /// </summary>
    public interface IScreen : IDisposable
    {
        // ---------------- Properties ----------------

        /// <summary>
        /// The current brightness of the screen
        /// on a scale from 0-100.
        /// </summary>
        byte Brightness { get; }

        /// <summary>
        /// Is the screen actually turned on or not?
        /// </summary>
        bool IsOn { get; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Populates the properties by reading values from the OS.
        /// </summary>
        void Refresh();

        /// <param name="newValue">
        /// If this value is greater than 100, it will be capped to 100.
        /// </param>
        void SetBrightness( byte newValue );

        void SetOn( bool newValue );
    }
}
