using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Aide.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("Error")]
        public IActionResult ErrorException()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewData["ExceptionMessage"] = exceptionDetails.Error.Message;
            return View("Error");
        }
    }
}
