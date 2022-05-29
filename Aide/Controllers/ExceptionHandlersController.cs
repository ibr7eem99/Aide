using Microsoft.AspNetCore.Mvc;

namespace Aide.Controllers
{
    public class ExceptionHandlersController : Controller
    {
        [Route("Error/Authentication")]
        public IActionResult AuthenticationFailure()
        {
            return View();
        }
    }
}
