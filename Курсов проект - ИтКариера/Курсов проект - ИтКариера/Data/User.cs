using Microsoft.AspNetCore.Identity;

namespace Курсов_проект___ИтКариера.Data
{
    public class User : IdentityUser<Guid>
    {
        public ICollection<BookAuthor> AuthoredBooks { get; set; } = new List<BookAuthor>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }

}
