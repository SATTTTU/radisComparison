using System.ComponentModel.DataAnnotations;

namespace AspireSample.ApiService.Models;

public class ChatUser
{
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}
