using System.ComponentModel.DataAnnotations;

namespace DP.Models
{
    public class RegInModel
    {
        [Required(ErrorMessage = "ФИО обязательно для заполнения")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я\s]+$", ErrorMessage = "ФИО может содержать только буквы и пробелы")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Телефон обязателен для заполнения")]
        [RegularExpression(@"^8\(\d{3}\)-\d{3}-\d{2}-\d{2}$", ErrorMessage = "Телефон должен быть в формате 8(XXX)-XXX-XX-XX")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [RegularExpression(@"^(?=.*\d).+$", ErrorMessage = "Пароль должен содержать хотя бы одну цифру")]
        public string Password { get; set; }
    }
}
