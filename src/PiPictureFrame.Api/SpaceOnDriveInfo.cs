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
    public sealed record SpaceOnDriveInfo(
        string DriveName,
        long AvailableFreeSpace,
        long TotalSize
    )
    {
        public long UsedBytes =>
            this.TotalSize - this.AvailableFreeSpace;

        public double UsedGigaBytes =>
            this.UsedBytes / 1000.0 / 1000.0 / 1000.0;

        public double AvailableFreeSpceGigaBytes =>
            this.AvailableFreeSpace / 1000.0 / 1000.0 / 1000.0;

        public double TotalSizeGigaBytes =>
            this.TotalSize / 1000.0 / 1000.0 / 1000.0;
    }
}
