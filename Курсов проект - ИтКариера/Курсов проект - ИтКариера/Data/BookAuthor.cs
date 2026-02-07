namespace Курсов_проект___ИтКариера.Data
{
    public class BookAuthor
    {
        public long BookId { get; set; }
        public Book Book { get; set; } = null!;

        public long UserId { get; set; }
        public User User { get; set; } = null!;
    }

}
