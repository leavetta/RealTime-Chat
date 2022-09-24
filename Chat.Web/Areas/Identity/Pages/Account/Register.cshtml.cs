using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Chat.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Chat.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            //[Required]
            //[StringLength(100, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.", MinimumLength = 5)]
            //[Display(Name = "Полное имя")]
            //public string FullName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.", MinimumLength = 5)]
            [Display(Name = "Имя пользователя")]
            public string UserName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Подтвердите пароль")]
            [Compare("Password", ErrorMessage = "Пароль и пароль подтверждения не совпадают.")]
            public string ConfirmPassword { get; set; }

            /*[Required]
            [Display(Name = "Фото профиля")]
            public string Avatar { get; set; }*/
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //var avatars = new string[] { "avatar1.png", "avatar2.png", "avatar3.png", "avatar4.png" };
                var avatars = new Dictionary<string, string>()
                {
                    ["default"] = "default.png",

                    ["a"] = "A.png",
                    ["b"] = "B.png",
                    ["c"] = "C.png",
                    ["d"] = "D.png",
                    ["e"] = "E.png",
                    ["f"] = "F.png",
                    ["g"] = "G.png",
                    ["h"] = "H.png",
                    ["i"] = "I.png",
                    ["j"] = "J.png",
                    ["k"] = "K.png",
                    ["l"] = "L.png",
                    ["m"] = "M.png",
                    ["n"] = "N.png",
                    ["o"] = "O.png",
                    ["p"] = "P.png",
                    ["q"] = "Q.png",
                    ["r"] = "R.png",
                    ["s"] = "S.png",
                    ["t"] = "T.png",
                    ["u"] = "U.png",
                    ["v"] = "V.png",
                    ["w"] = "W.png",
                    ["x"] = "X.png",
                    ["y"] = "Y.png",
                    ["z"] = "Z.png",

                    ["а"] = "А.png",
                    ["б"] = "Б.png",
                    ["в"] = "В.png",
                    ["г"] = "Г.png",
                    ["д"] = "Д.png",
                    ["е"] = "Е.png",
                    ["ё"] = "Ё.png",
                    ["ж"] = "Ж.png",
                    ["з"] = "З.png",
                    ["и"] = "И.png",
                    ["й"] = "Й.png",
                    ["к"] = "К.png",
                    ["л"] = "Л.png",
                    ["м"] = "М.png",
                    ["н"] = "Н.png",
                    ["о"] = "О.png",
                    ["п"] = "П.png",
                    ["р"] = "Р.png",
                    ["с"] = "С.png",
                    ["т"] = "Т.png",
                    ["у"] = "У.png",
                    ["ф"] = "Ф.png",
                    ["х"] = "Х.png",
                    ["ц"] = "Ц.png",
                    ["ч"] = "Ч.png",
                    ["ш"] = "Ш.png",
                    ["щ"] = "Щ.png",
                    ["ъ"] = "Ъ.png",
                    ["ы"] = "Ы.png",
                    ["ь"] = "Ь.png",
                    ["э"] = "Э.png",
                    ["ю"] = "Ю.png",
                    ["я"] = "Я.png"
                };
                var nameUser = Input.UserName;

                var index = nameUser.Substring(0, 1).ToLower();

                string avatarName = "";
                if (avatars.TryGetValue(index, out avatarName))
                {
                    
                }
                else
                {
                    avatarName = avatars["default"];
                }

                var user = new ApplicationUser { UserName = Input.UserName, Email = Input.Email, Avatar = avatarName };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Пользователь создал новую учетную запись с паролем.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Подтвердите ваш email",
                        $"Пожалуйста, подтвердите свой аккаунт через <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>нажмите здесь</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
