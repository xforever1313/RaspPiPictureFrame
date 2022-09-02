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
    public class SettingsController : Controller
    {
        // ---------------- Fields ----------------

        private readonly IPiPictureFrameApi api;

        // ---------------- Constructor ----------------

        public SettingsController( IPiPictureFrameApi api )
        {
            this.api = api;
        }

        // ---------------- Functions ----------------

        public IActionResult Index()
        {
            if( this.TempData["desired_model"] is SettingsModel model )
            {
                // Use desired model, which is settings the user put in.
            }
            else
            {
                model = this.api.Settings.Settings.FromApiConfig(
                    this.TempData["info_message"]?.ToString() ?? string.Empty,
                    this.TempData["error_message"]?.ToString() ?? string.Empty
                );
            }

            return View( model );
        }

        [HttpPost]
        public IActionResult Update( [FromForm] SettingsModel model )
        {
            try
            {
                PiPictureFrameConfig config = model.ToApiConfig();
                this.api.Settings.UpdateSettings( config );
                this.TempData["info_message"] = "Settings updated successfully!";
            }
            catch( Exception e )
            {
                this.TempData["error_message"] = e.Message;
                this.TempData["desired_model"] = model;
            }

            return RedirectToAction( nameof( Index ) );
        }
    }
}
