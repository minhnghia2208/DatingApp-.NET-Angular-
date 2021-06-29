using System.Collections.Generic;
using API.Entity;

namespace API.DTOs
{
    public class MemberUpdateDTO
    {
        public string Introduction { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public List<Waitlist> Waitlist{ get; set; }
        public int temp { get; set; }
    }
}