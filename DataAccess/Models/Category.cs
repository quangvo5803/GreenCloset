using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DataAccess.Models;

namespace DataAccess.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [DisplayName("Category Name")]
        public required string CategoryName { get; set; }

        //Navigation property
        public virtual ICollection<Product>? Products { get; set; }
    }
}
