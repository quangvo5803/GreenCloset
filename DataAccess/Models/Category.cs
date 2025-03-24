using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GreenCloset.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [MaxLength(50)]
        [DisplayName("Category Name")]
        public required string CategoryName { get; set; }
    }
}
