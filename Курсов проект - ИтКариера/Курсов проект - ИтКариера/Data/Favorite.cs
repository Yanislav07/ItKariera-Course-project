namespace Курсов_проект___ИтКариера.Data
{
    public class Favorite
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid BookId { get; set; }
        public Book Book { get; set; }
    }

}
