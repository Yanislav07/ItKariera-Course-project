namespace Курсов_проект___ИтКариера.Data
{
    public class BookAuthor
    {
        public int BookId { get; set; }
        public Book Book { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }

}
