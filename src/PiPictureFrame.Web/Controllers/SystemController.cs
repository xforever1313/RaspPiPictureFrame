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

using Microsoft.AspNetCore.Mvc;
using PiPictureFrame.Api;
using PiPictureFrame.Web.Models;

namespace PiPictureFrame.Web.Controllers
{
    public class SystemController : Controller
    {
        // ---------------- Fields ----------------

        private readonly IPiPictureFrameApi api;

        private readonly InProcessLogSink logSink;

        // ---------------- Constructor ----------------

        public SystemController( IPiPictureFrameApi api, InProcessLogSink logSink )
        {
            this.api = api;
            this.logSink = logSink;
        }

        // ---------------- Functions ----------------

        public IActionResult Index()
        {
            var model = new SystemModel(
                Api: this.api,
                InfoMessage: this.TempData["info_message"]?.ToString() ?? string.Empty,
                ErrorMessage: this.TempData["error_message"]?.ToString() ?? string.Empty
            );

            return View( model );
        }

        public IActionResult SpaceLeft()
        {
            var model = new SpaceLeftModel( this.api.System.GetDriveInfo() );
            return View( model );
        }

        public IActionResult Log()
        {
            return View( this.logSink );
        }

        [HttpPost]
        public IActionResult Sleep()
        {
            try
            {
                this.api.Screen.SetOn( this.api.Screen.IsOn == false );

                bool isOn = this.api.Screen.IsOn;
                this.TempData["info_message"] = $"Screen set to {( isOn ? "On" : "Off" )}.";
            }
            catch( Exception e )
            {
                this.TempData["error_message"] = e.Message;
            }

            return RedirectToAction( nameof( Index ) );
        }

        [HttpPost]
        public IActionResult Exit()
        {
            try
            {
                this.api.System.ExitProcess();
                this.TempData["info_message"] = "Shutting down.  Desktop should return in a few seconds.";
            }
            catch( Exception e )
            {
                this.TempData["error_message"] = e.Message;
            }

            return RedirectToAction( nameof( Index ) );
        }
    }
}
