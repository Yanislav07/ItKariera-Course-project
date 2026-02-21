using System.ComponentModel.DataAnnotations;

namespace Курсов_проект___ИтКариера.Data
{
    public class Review
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid BookId { get; set; }
        public Book Book { get; set; }

        public double Rating { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
