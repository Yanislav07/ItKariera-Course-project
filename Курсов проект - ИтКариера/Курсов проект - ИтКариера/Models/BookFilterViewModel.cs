using Microsoft.AspNetCore.Mvc.Rendering;
using Курсов_проект___ИтКариера.Data;

namespace Курсов_проект___ИтКариера.Models
{
    public class BookFilterViewModel
    {
        public IEnumerable<Book> Books { get; set; }

        // Filter Parameters
        public string? SearchAuthor { get; set; }
        public int? SearchYear { get; set; }
        public Guid? SearchCategoryId { get; set; }
        public double? MinRating { get; set; }

        // Dropdown Lists
        public SelectList CategoryList { get; set; }
        public List<int> YearList { get; set; }
    }
}
