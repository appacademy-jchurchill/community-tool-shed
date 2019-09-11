using CommunityToolShedMvc.Data;
using CommunityToolShedMvc.Models;
using CommunityToolShedMvc.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CommunityToolShedMvc.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            var viewModel = new LoginViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel viewModel)
        {
            if (ModelState.IsValidField("Email") && ModelState.IsValidField("Password"))
            {
                Person person = DatabaseHelper.RetrieveSingle<Person>(@"
                    select HashedPassword
                    from Person
                    where Email = @Email
                ",
                    new SqlParameter("@Email", viewModel.Email));

                if (person == null || !BCrypt.Net.BCrypt.Verify(viewModel.Password, person.HashedPassword))
                {
                    ModelState.AddModelError("", "Login failed.");
                }
            }

            if (ModelState.IsValid)
            {
                FormsAuthentication.SetAuthCookie(viewModel.Email, false);
                return RedirectToAction("Index", "Home");
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}