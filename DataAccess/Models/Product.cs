using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        public double Price { get; set; }
        public int? ProductAvatarId { get; set; }

        //Foreign key

        [ForeignKey("ProductAvatarId")]
        public ItemImage? ProductAvatar { get; set; }

        //Navigation property
        public virtual ICollection<Category>? Categories { get; set; }

        public virtual ICollection<ItemImage>? ProductImages { get; set; }

        public virtual ICollection<Feedback>? Feedbacks { get; set; }
    }
}
