using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EcommerceDEPI.Models;


namespace EcommerceDEPI.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public string Email { get; set; }
        public string ProfilePictureUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile ProfilePicture { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            Email = await _userManager.GetEmailAsync(user);
            ProfilePictureUrl = string.IsNullOrEmpty(user.Picture) ? "/images/default-profile.png" : $"/uploads/{user.Picture}";

            Input = new InputModel
            {
                FullName = user.Name,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                ProfilePictureUrl = string.IsNullOrEmpty(user.Picture) ? "/images/default-profile.png" : $"/uploads/{user.Picture}";
                return Page();
            }

            bool hasChanges = false;

            if (Input.FullName != user.Name)
            {
                user.Name = Input.FullName;
                hasChanges = true;
            }

            if (Input.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when updating phone number.";
                    return RedirectToPage();
                }
                user.PhoneNumber = Input.PhoneNumber;
                hasChanges = true;
            }

            if (Input.ProfilePicture != null)
            {
                var allowedExtensions = new[] { ".jpg", ".png" };
                var fileExtension = Path.GetExtension(Input.ProfilePicture.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    StatusMessage = "Only JPG or PNG images are allowed.";
                    return RedirectToPage();
                }

                if (Input.ProfilePicture.Length > 2 * 1024 * 1024)
                {
                    StatusMessage = "Image size must be less than 2MB.";
                    return RedirectToPage();
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePicture.CopyToAsync(fileStream);
                }

                user.Picture = uniqueFileName;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _userManager.UpdateAsync(user);
                await _signInManager.SignOutAsync();
                await _signInManager.RefreshSignInAsync(user);
                StatusMessage = "Your profile has been updated successfully.";
            }
            else
            {
                StatusMessage = "No changes detected.";
            }

            return RedirectToPage();
        }
    }
}
