namespace Курсов_проект___ИтКариера.Data
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
    }

}
