using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class RegisterViewModel
    {
        [Required]
        [DisplayName("Username")]
        public string? UserName { get; set; }
        [Required]
        [DisplayName("Ad Soyad")]
        public string? Name { get; set; }
        [Required]
        [EmailAddress]
        [DisplayName("Eposta")]
        public string? Email { get; set; }
        [Required]
        [StringLength(10,ErrorMessage ="Minimum 6 maksimum 10 karakter olmalıdır",MinimumLength =6)]
        [DataType(DataType.Password)]
        [DisplayName("Parola")]
        public string? Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Parola Tekrar")]
        [Compare("Password",ErrorMessage ="Parolanız eşleşmiyor")]
        public string? ConfirmPassword { get; set; }
    }
}
