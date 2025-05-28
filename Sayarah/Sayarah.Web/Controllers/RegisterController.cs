using Microsoft.AspNetCore.Mvc;
namespace Sayarah.Web.Controllers
{
    public class RegisterController : SayarahControllerBase
    {
        public ActionResult Index()
        {
            return View("~/App/Register/Layout/layout.cshtml");
        }

        public ActionResult ForgetPassword()
        {
            return View("~/App/Register/Layout/layout.cshtml");
        }


    }
}