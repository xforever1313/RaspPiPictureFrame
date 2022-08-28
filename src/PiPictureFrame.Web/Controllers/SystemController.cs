using Microsoft.AspNetCore.Mvc;
using PiPictureFrame.Api;
using PiPictureFrame.Web.Models;

namespace PiPictureFrame.Web.Controllers
{
    public class SystemController : Controller
    {
        // ---------------- Fields ----------------

        private readonly IPiPictureFrameApi api;

        // ---------------- Constructor ----------------

        public SystemController( IPiPictureFrameApi api )
        {
            this.api = api;
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
