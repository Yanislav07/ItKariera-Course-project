namespace Курсов_проект___ИтКариера.Data
{
    public class Book
    {
        public int Id { get; set; }

        public string? Title { get; set; }
        public string Description { get; set; }
        public DateTime? PublishedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }

}
