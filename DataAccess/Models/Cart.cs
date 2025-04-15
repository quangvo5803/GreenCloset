using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
        public int Count { get; set; }
        public Guid UserId { get; set; } // Guid

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
