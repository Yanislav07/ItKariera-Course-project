using Microsoft.AspNetCore.Mvc;
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

        public IActionResult AddNewBook()
        {
            return View();
        }

        // TO BE DELETED EVENTUALLY
        public IActionResult Create()
        {
            return View(new BookViewModel());
        }

        // POST: Books/Create
        [HttpPost]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            _context.Books.Add(model.Book);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "The book was added successfully!";
            return RedirectToAction(nameof(AddNewBook));
        }
    }
}
