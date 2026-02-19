using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Курсов_проект___ИтКариера.Data;
using Курсов_проект___ИтКариера.Models;

namespace Курсов_проект___ИтКариера.Controllers
{
    public class ModeratorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModeratorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Moderator/AddNewBook
        public IActionResult AddNewBook()
        {
            var model = new BookViewModel();
            PopulateLists(model);
            return View(model);
        }

        // POST: Moderator/AddNewBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewBook(BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateLists(model);
                return View(model);
            }

            // Add book first
            _context.Books.Add(model.Book);
            await _context.SaveChangesAsync();

            // Add authors
            foreach (var authorId in model.SelectedAuthorIds)
            {
                _context.BookAuthors.Add(new BookAuthor
                {
                    BookId = model.Book.Id,
                    UserId = authorId
                });
            }

            // Add categories
            foreach (var categoryId in model.SelectedCategoryIds)
            {
                _context.BookCategories.Add(new BookCategory
                {
                    BookId = model.Book.Id,
                    CategoryId = categoryId
                });
            }

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "The book was added successfully!";
            return RedirectToAction(nameof(AddNewBook));
        }

        private void PopulateLists(BookViewModel model)
        {
            model.AuthorList = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
                }).ToList();

            model.CategoryList = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
        }
    }
}
