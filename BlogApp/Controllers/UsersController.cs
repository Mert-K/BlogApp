using BlogApp.Data.Abstract;
using BlogApp.Entity;
using BlogApp.Models;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Posts");
            }
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user =await _userRepository.Users.FirstOrDefaultAsync(x=>x.UserName == model.UserName || x.Email == model.Email);
                if (user == null)
                {
                    _userRepository.CreateUser(new User
                    {
                        UserName=model.UserName,
                        Name=model.Name,
                        Email = model.Email,
                        Password=model.Password,
                        Image = "avatar.jpg",
                    });
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("","Username ya da Email kullanımda.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //Logout (Cookie'yi siler)
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isUser = _userRepository.Users.FirstOrDefault(x => x.Email == model.Email && x.Password == model.Password);

                if (isUser != null)
                {
                    var userClaims = new List<Claim>();
                    userClaims.Add(new Claim(ClaimTypes.NameIdentifier, isUser.UserId.ToString())); //Verilen bu claim'i herhangi bir controller'ın action metodunda geri alabilmek için User.FindFirstValue(ClaimTypes.NameIdentifier); komutunu kullanıyoruz. Bu komut bize User'ın UserId'sini veriyor.Posts Controller AddComment action metodunda yapıldı. 
                    userClaims.Add(new Claim(ClaimTypes.Name, isUser.UserName ?? ""));
                    userClaims.Add(new Claim(ClaimTypes.GivenName, isUser.Name ?? ""));
                    userClaims.Add(new Claim(ClaimTypes.UserData, isUser.Image ?? ""));

                    if (isUser.Email == "info@sadikturan.com") //Eğer kullanıcının maili info@sadikturan.com ise kullanıcıya Role Claim'i ekleniyor. Bu Claim'in value'su admin
                    {
                        userClaims.Add(new Claim(ClaimTypes.Role, "admin"));
                    }

                    var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //Logout (Cookie'yi siler)

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties
                        );

                    return RedirectToAction("Index", "Posts");
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı adı veya şifre yanlış"); //Hata mesajı <div asp-validation-summary="All veya ModelOnly"></div> kısmında yazar. ModelOnly seviyesinde hata mesajı ekler.
                }
            }

            return View(model);
        }

        public IActionResult Profile(string username)
        {
            if(string.IsNullOrEmpty(username))
            {
                return NotFound();
            }
            var user = _userRepository
                       .Users
                       .Include(x => x.Posts)
                       .Include(x => x.Comments)
                       .ThenInclude(x => x.Post)
                       .FirstOrDefault(x=>x.UserName==username);

            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

    }
}
