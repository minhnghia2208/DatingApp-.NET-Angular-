using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class TokenDTO
    {
        [Required]
        public string Access_token { get; set; }
    }
}