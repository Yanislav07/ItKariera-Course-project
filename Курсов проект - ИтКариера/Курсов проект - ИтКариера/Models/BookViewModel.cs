using Microsoft.AspNetCore.Mvc.Rendering;
using Курсов_проект___ИтКариера.Data;

namespace Курсов_проект___ИтКариера.Models
{
    public class BookViewModel
    {
        public Book Book { get; set; } = new Book();

        
        public List<Guid> SelectedCategoryIds { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();
    }
}
