namespace Курсов_проект___ИтКариера.Data
{
    public class BookCategory
    {
        public long BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }

}
