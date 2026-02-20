using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Курсов_проект___ИтКариера.Data;
using Курсов_проект___ИтКариера.Models;

namespace Курсов_проект___ИтКариера.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
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

        public async Task<IActionResult> Catalogue()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        //Search Functionality
        public async Task<IActionResult> ShowSearchResults(string SearchBook)
        {
            return View("Index", await _context.Books.Where(j => j.Title != null && j.Title == SearchBook).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.User)
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .Include(b => b.Reviews)
                .Include(b => b.Favorites)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

    }
}
