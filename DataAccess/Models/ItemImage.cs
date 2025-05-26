using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class ItemImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string ImagePath { get; set; }

        [Required]
        public required string PublicId { get; set; }
        public int? ProductId { get; set; }
        public int? FeedbackId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [ForeignKey("FeedbackId")]
        public Feedback? Feedback { get; set; }
    }
}
