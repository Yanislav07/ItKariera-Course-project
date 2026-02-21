using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
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

        public async Task<IActionResult> Details(Guid id)
        {
            var book = await _context.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.User)
                .Include(b => b.BookCategories)
                    .ThenInclude(bc => bc.Category)
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.User)
                .Include(b => b.Favorites)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        //Book deletion
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book deleted successfully!";

            return RedirectToAction("Catalogue");
        }

        //Comments Controller
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Guid BookId, double Rating, string Comment)
        {

            if (string.IsNullOrWhiteSpace(Comment) || Rating < 0)
            {
                return RedirectToAction("Details", new { id = BookId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = new Review
            {
                BookId = BookId,
                UserId = Guid.Parse(userId),
                Rating = Rating,
                Comment = Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = BookId });
        }

        //Delete Comment Controller
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId, int bookId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (review.UserId != Guid.Parse(userId) &&
                !User.IsInRole("Admin") &&
                !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = bookId });
        }

        //Favorites Controller
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(Guid BookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _context.Favorites.FirstOrDefaultAsync(f => f.BookId == BookId && f.UserId == Guid.Parse(userId));

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                TempData["FavoriteMessage"] = "Book removed from favorites";
            }
            else
            {
                _context.Favorites.Add(new Favorite { BookId = BookId, UserId = Guid.Parse(userId) });
                TempData["FavoriteMessage"] = "Book saved to favorites";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = BookId });
        }

        [Authorize]
        public async Task<IActionResult> Favorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favoriteBooks = await _context.Favorites
                .Where(f => f.UserId == Guid.Parse(userId))
                .Include(f => f.Book)            
                    .ThenInclude(b => b.BookAuthors)   
                .Include(f => f.Book)
                    .ThenInclude(b => b.BookCategories) 
                .Select(f => f.Book)                    
                .ToListAsync();

            return View(favoriteBooks);
        }
    


    }
}
