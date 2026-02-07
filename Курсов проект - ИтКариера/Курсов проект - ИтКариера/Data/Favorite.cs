namespace Курсов_проект___ИтКариера.Data
{
    public class Favorite
    {
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        public long BookId { get; set; }
        public Book Book { get; set; } = null!;
    }

}
