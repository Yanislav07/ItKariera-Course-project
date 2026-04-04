namespace Курсов_проект___ИтКариера.Data
{
    public class ReadingList
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public User User { get; set; }
        public ICollection<ReadingListBook> ReadingListBooks { get; set; } = new List<ReadingListBook>();
    }
}
