using Lab1.Models;
using Lab1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using System.Diagnostics;

namespace Lab1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmailSender _emailSender;

        public HomeController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (culture != "en" && culture != "uk")
            {
                culture = "en";
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture)));

            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Action(
                    nameof(Index),
                    "Home")!;
            }

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(ContactMessage model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var emailBody = $"""
        New message from EduNova Academy

        Name: {model.Name}
        Email: {model.Email}
        Subject: {model.Subject}

        Message:
        {model.Message}
        """;

            await _emailSender.SendEmailAsync(
                model.Subject,
                emailBody);

            ViewBag.MessageSent = true;

            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
