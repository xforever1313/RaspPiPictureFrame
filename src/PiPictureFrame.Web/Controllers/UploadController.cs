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
    public class UploadController : Controller
    {
        // ---------------- Fields ----------------

        internal const string CreateNewDirectoryCode = "{^CREATE_NEW$}";

        private readonly IPiPictureFrameApi api;

        // ---------------- Constructor ----------------

        public UploadController( IPiPictureFrameApi api )
        {
            this.api = api;
        }

        // ---------------- Functions ----------------

        public IActionResult Index()
        {
            var model = new PictureDirectoryModel(
                DirectoryNames: new List<string> { "Hello", "World", "How", "Are", "You" },
                InfoMessage: this.TempData["info_message"]?.ToString() ?? string.Empty,
                ErrorMessage: this.TempData["error_message"]?.ToString() ?? string.Empty
            );

            return View( model );
        }
    }
}
