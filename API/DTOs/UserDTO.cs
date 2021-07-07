namespace API.DTOs
{
    public class UserDTO
    {
        public string UserName { get; set; }
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }
        public string PhotoUrl { get; set; }
        public string KnownAs { get; set; }
        public string Gender { get; set; }
    }
}