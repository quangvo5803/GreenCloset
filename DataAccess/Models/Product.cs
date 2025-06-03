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
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0")]
        public double Price { get; set; }

        [Range(
            0,
            double.MaxValue,
            ErrorMessage = "Giá  cọc phải lớn hơn 0 hoặc bằng 0 nếu không cần cọc"
        )]
        public double DepositPrice { get; set; } = 0;
        public bool Available { get; set; } = true;
        public ProductColor? Color { get; set; }
        public int? ProductAvatarId { get; set; }
        public int RentalCount { get; set; } = 0;
        public IList<SizeClother>? SizeClother { get; set; }
        public List<int>? SizeShoe { get; set; }
        public Guid? UserId { get; set; }

        //Foreign key

        [ForeignKey("ProductAvatarId")]
        public ItemImage? ProductAvatar { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        //Navigation property
        public virtual ICollection<Category>? Categories { get; set; }

        public virtual ICollection<ItemImage>? ProductImages { get; set; }

        public virtual ICollection<Feedback>? Feedbacks { get; set; }
    }

    public enum SizeClother
    {
        S = 1,
        M = 2,
        L = 3,
        XL = 4,
        XXL = 5,
    }

    // Enum các màu phổ biến
    public enum ProductColor
    {
        Black,
        White,
        Red,
        Blue,
        Green,
        Yellow,
        Pink,
        Brown,
        Gray,
        Orange,
        Purple,
        Navy,
        Beige,
        Maroon,
        Olive,
        Teal,
        Khaki,
        Coral,
        Mint,
        Burgundy,
        Mustard,
        SkyBlue,
        Lavender,
        Cream,
    }
}
