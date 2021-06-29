using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entity
{
    [Table("Waitlist")]
    public class Waitlist
    {
        public int Id { get; set; }
        public int WaitListId { get; set; }
        public AppUser AppUser { get; set; }
        public int AppUserId { get; set; }
    }
}