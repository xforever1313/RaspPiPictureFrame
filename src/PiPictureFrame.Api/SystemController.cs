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

using Microsoft.Extensions.Logging;

namespace PiPictureFrame.Api
{
    /// <summary>
    /// Handles the system itself, such as exiting the process,
    /// rebooting, or shutting down.
    /// </summary>
    public class SystemController : IDisposable
    {
        // ---------------- Events ----------------

        public event Action? OnExitRequest;

        // ---------------- Fields ----------------

        private readonly ILogger log;

        // ---------------- Constructor ----------------

        public SystemController( ILogger log )
        {
            this.log = log;
        }

        // ---------------- Functions ----------------

        void IDisposable.Dispose()
        {
            // Nothing to do.
        }

        public void ExitProcess()
        {
            this.log.LogInformation( "Request made to exit the process" );
            this.OnExitRequest?.Invoke();
        }
    }
}
