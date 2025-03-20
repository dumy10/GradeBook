using System.ComponentModel.DataAnnotations;

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; }
}