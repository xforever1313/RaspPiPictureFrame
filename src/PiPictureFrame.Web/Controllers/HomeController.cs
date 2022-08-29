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

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PiPictureFrame.Api;
using PiPictureFrame.Web.Models;

namespace PiPictureFrame.Web.Controllers
{
    public class HomeController : Controller
    {
        // ---------------- Fields ----------------

        private readonly IPiPictureFrameApi api;

        // ---------------- Constructor ----------------

        public HomeController( IPiPictureFrameApi api)
        {
            this.api = api;
        }

        // ---------------- Functions ----------------

        public IActionResult Index()
        {
            var model = new HomeModel(
                Api: this.api,
                InfoMessage: this.TempData["info_message"]?.ToString() ?? string.Empty,
                ErrorMessage: this.TempData["error_message"]?.ToString() ?? string.Empty
            );

            return View( model );
        }

        [HttpPost]
        public IActionResult ChangePicture()
        {
            try
            {
                this.api.Renderer.GoToNextPicture();
                this.TempData["info_message"] = $"Picture Changed!";
            }
            catch( Exception e )
            {
                this.TempData["error_message"] = e.Message;
            }

            return RedirectToAction( nameof( Index ) );
        }

        [ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
        public IActionResult Error()
        {
            return View( new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier } );
        }
    }
}