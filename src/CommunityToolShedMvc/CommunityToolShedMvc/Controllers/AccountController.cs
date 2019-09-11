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
        public ActionResult Register()
        {
            var viewModel = new RegisterViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(RegisterViewModel viewModel)
        {
            // TODO Validate that the provided email
            // isn't already associated with a person.

            if (ModelState.IsValid)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(viewModel.Password, 12);

                DatabaseHelper.Insert(@"
                    insert Person (
                        Name,
                        Email,
                        HashedPassword
                    ) values (
                        @Name,
                        @Email,
                        @HashedPassword
                    )
                ", 
                    new SqlParameter("@Name", viewModel.Name),
                    new SqlParameter("@Email", viewModel.Email),
                    new SqlParameter("@HashedPassword", hashedPassword));

                FormsAuthentication.SetAuthCookie(viewModel.Email, false);
                return RedirectToAction("Index", "Home");
            }

            return View(viewModel);
        }

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