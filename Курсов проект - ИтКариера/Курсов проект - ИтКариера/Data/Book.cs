namespace Курсов_проект___ИтКариера.Data
{
    public class Book
    {
        public long Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Isbn { get; set; }
        public DateTime? PublishedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }

}
