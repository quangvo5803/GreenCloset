using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataAccess.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [Required(ErrorMessage = "Tên thư mục không được để trống")]
        public required string CategoryName { get; set; }

        //Navigation property
        [JsonIgnore]
        public virtual ICollection<Product>? Products { get; set; }
    }
}
