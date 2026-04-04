using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<IActionResult> Catalogue(string? author, int? year, Guid? categoryId, double? minRating)
        {
            // This starts the database query. 'Include' ensures we get the related data for Authors and Categories.
            var query = _context.Books
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.User)
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .Include(b => b.Reviews)
                .AsQueryable();

            // Filtering logic: These only run if the user actually selected something in the UI.
            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(b => b.AuthorName != null && b.AuthorName.Contains(author));
            }

            if (year.HasValue)
            {
                query = query.Where(b => b.PublishedDate.HasValue && b.PublishedDate.Value.Year == year);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.BookCategories.Any(bc => bc.CategoryId == categoryId));
            }

            if (minRating.HasValue)
            {
                // This calculates the average rating of all reviews for each book and compares it to the filter.
                query = query.Where(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) >= minRating : false);
            }

            // This creates the data the View needs to build the dropdown menus.
            var model = new BookFilterViewModel
            {
                Books = await query.ToListAsync(),
                CategoryList = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId),
                YearList = await _context.Books
                    .Where(b => b.PublishedDate.HasValue)
                    .Select(b => b.PublishedDate.Value.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync(),
                SearchAuthor = author,
                SearchYear = year,
                SearchCategoryId = categoryId,
                MinRating = minRating
            };

            return View(model);
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                ViewBag.UserLists = await _context.ReadingLists
                    .Where(l => l.UserId == Guid.Parse(userId))
                    .ToListAsync();
            }

            return View(book);
        }

        //Book deletion
        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
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
        public async Task<IActionResult> DeleteReview(Guid reviewId, Guid bookId)
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

        [Authorize]
        public async Task<IActionResult> MyLists()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var favList = await _context.ReadingLists
                .FirstOrDefaultAsync(l => l.UserId == userId && l.Name == "Favorites");
            if (favList == null)
            {
                _context.ReadingLists.Add(new ReadingList { Name = "Favorites", UserId = userId });
                await _context.SaveChangesAsync();
            }

            var lists = await _context.ReadingLists
                .Where(l => l.UserId == userId)
                .Include(l => l.ReadingListBooks)
                    .ThenInclude(rlb => rlb.Book)
                .OrderBy(l => l.Name != "Favorites") 
                .ToListAsync();

            return View(lists);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateList(string name, Guid bookId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var list = new ReadingList { Name = name, UserId = userId };
            _context.ReadingLists.Add(list);
            await _context.SaveChangesAsync();

            if (bookId != Guid.Empty)
            {
                _context.ReadingListBooks.Add(new ReadingListBook { ReadingListId = list.Id, BookId = bookId });
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = bookId });
            }

            return RedirectToAction("MyLists");
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToList(Guid bookId, Guid listId)
        {
            var exists = await _context.ReadingListBooks
                .AnyAsync(x => x.BookId == bookId && x.ReadingListId == listId);
            if (!exists)
            {
                _context.ReadingListBooks.Add(new ReadingListBook { BookId = bookId, ReadingListId = listId });
                await _context.SaveChangesAsync();
                TempData["ListMessage"] = "Added to list!";
            }
            else
            {
                TempData["ListMessage"] = "Book is already in this list.";
            }
            return RedirectToAction("Details", new { id = bookId });
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromList(Guid bookId, Guid listId)
        {
            var entry = await _context.ReadingListBooks
                .FirstOrDefaultAsync(x => x.BookId == bookId && x.ReadingListId == listId);
            if (entry != null)
            {
                _context.ReadingListBooks.Remove(entry);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("MyLists");
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteList(Guid listId)
        {
            var list = await _context.ReadingLists
                .Include(l => l.ReadingListBooks)
                .FirstOrDefaultAsync(l => l.Id == listId);
            if (list != null)
            {
                _context.ReadingListBooks.RemoveRange(list.ReadingListBooks);
                _context.ReadingLists.Remove(list);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("MyLists");
        }
    }
}
