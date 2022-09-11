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

using System.Text;
using Microsoft.AspNetCore.Mvc;
using PiPictureFrame.Api;
using PiPictureFrame.Web.Models;

namespace PiPictureFrame.Web.Controllers
{
    public class UploadController : Controller
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Give this some crazy field that some sane person won't
        /// use to make a new directory.  That way we know
        /// if they want to create a new directory without relying
        /// on using null.
        /// </summary>
        internal const string CreateNewDirectoryCode = "{^CREATE_NEW_DO_NOT_CALL_A_DIRECTORY_THIS$}";

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
                DirectoryNames: this.api.FileManager.GetSubDirectoryNames(),
                InfoMessage: this.TempData["info_message"]?.ToString() ?? string.Empty,
                ErrorMessage: this.TempData["error_message"]?.ToString() ?? string.Empty
            );

            return View( model );
        }

        [HttpPost]
        public IActionResult DoUpload( [FromForm] UploadModel model )
        {
            try
            {
                string uploadDirectory = model.UploadDirectory;
                if( CreateNewDirectoryCode == uploadDirectory )
                {
                    uploadDirectory = model.NewDirectoryName;
                }

                if( string.IsNullOrWhiteSpace( uploadDirectory ) )
                {
                    throw new ArgumentException(
                        "Upload Directory Field can not be empty or just whitespace, please type something in this field."
                    );
                }

                var builder = new StringBuilder();

                foreach( IFormFile file in model.FilesToUpload )
                {
                    try
                    {
                        if( file.ContentType.StartsWith( "image", StringComparison.OrdinalIgnoreCase ) == false )
                        {
                            throw new InvalidOperationException( "Not an image file" );
                        }

                        using( Stream istream = file.OpenReadStream() )
                        {
                            this.api.FileManager.UploadPictureToDirectory(
                                uploadDirectory,
                                istream,
                                file.FileName
                            );
                        }
                        builder.AppendLine(
                            $"Success - Uploaded '{file.FileName}' to '{uploadDirectory}'."
                        );
                    }
                    catch( Exception e )
                    {
                        builder.AppendLine(
                            $"Error - Could not upload '{file.FileName}' to '{uploadDirectory}': {e.Message}."
                        );
                    }
                }

                this.TempData["info_message"] = builder.ToString();
            }
            catch( Exception e )
            {
                this.TempData["error_message"] = e.Message;
            }

            return RedirectToAction( nameof( Index ) );
        }
    }
}
