namespace Курсов_проект___ИтКариера.Data
{
    public class Favorite
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }
    }

}
