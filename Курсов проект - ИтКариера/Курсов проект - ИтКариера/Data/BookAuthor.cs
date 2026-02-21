namespace Курсов_проект___ИтКариера.Data
{
    public class BookAuthor
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }

}
