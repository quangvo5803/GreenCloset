using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;

        public string? PhoneNumber { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        public string? ComfirmationToken { get; set; }
        public string? UserName { get; set; }

        [DisplayName("Ngày sinh: ")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/mm/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DOB { get; set; }
        public Gender? Gender { get; set; }
        public string? Address { get; set; }

        [Required]
        public UserRole Role { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Other,
    }

    public enum UserRole
    {
        Admin,
        Customer,
        Lessor,
    }
}
