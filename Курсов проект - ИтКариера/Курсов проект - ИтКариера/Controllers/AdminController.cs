using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Курсов_проект___ИтКариера.Data;
using Курсов_проект___ИтКариера.Models;

namespace Курсов_проект___ИтКариера.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;

        public AdminController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoleList = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                // Administrators are being skipped since their role's not supposed to undergo any alterations
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    continue;
                }

                // Fetsch users' roles
                var roles = await _userManager.GetRolesAsync(user);

                userRoleList.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()
                });
            }

            return View(userRoleList);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Remove all current roles to prevent role-stacking
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add the new role
            var result = await _userManager.AddToRoleAsync(user, newRole);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = $"Successfully updated {user.Email} to {newRole}.";
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
