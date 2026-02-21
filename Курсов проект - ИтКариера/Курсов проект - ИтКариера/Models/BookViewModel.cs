using Microsoft.AspNetCore.Mvc.Rendering;
using Курсов_проект___ИтКариера.Data;

namespace Курсов_проект___ИтКариера.Models
{
    public class BookViewModel
    {
        public Book Book { get; set; } = new Book();

        
        public List<Guid> SelectedAuthorIds { get; set; }        
        public List<Guid> SelectedCategoryIds { get; set; }

        public IEnumerable<SelectListItem> AuthorList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();
    }
}
