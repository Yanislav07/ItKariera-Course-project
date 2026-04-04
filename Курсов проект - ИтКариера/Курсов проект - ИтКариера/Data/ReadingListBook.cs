namespace Курсов_проект___ИтКариера.Data
{
    public class ReadingListBook
    {
        public Guid ReadingListId { get; set; }
        public ReadingList ReadingList { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; }
    }
}
