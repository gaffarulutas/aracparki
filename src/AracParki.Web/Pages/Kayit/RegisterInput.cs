using System.ComponentModel.DataAnnotations;
using AracParki.Application.Accounts;

namespace AracParki.Web.Pages.Kayit;

public sealed class RegisterInput : IValidatableObject
{
    [Required(ErrorMessage = "Ad gerekli.")]
    [StringLength(40, MinimumLength = 2, ErrorMessage = "Ad 2–40 karakter olmalı.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad gerekli.")]
    [StringLength(40, MinimumLength = 2, ErrorMessage = "Soyad 2–40 karakter olmalı.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta gerekli.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre gerekli.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalı.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı gerekli.")]
    [Compare(nameof(Password), ErrorMessage = "Şifreler eşleşmiyor.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre tekrar")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Devam etmek için şartları kabul edin.")]
    [Display(Name = "Şartlar")]
    public bool AcceptTerms { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var displayName = $"{FirstName} {LastName}".Trim();
        foreach (var error in PasswordRules.Validate(Password, displayName, Email))
        {
            yield return new ValidationResult(error, [nameof(Password)]);
        }
    }
}
