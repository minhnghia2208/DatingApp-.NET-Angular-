using System;
using System.Collections.Generic;
using API.Entity;

namespace API.DTOs
{
    public class MemberDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordHarsh { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string PhotoUrl { get; set; }
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string  Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public ICollection<PhotoDTO> Photos { get; set; }
        public List<Waitlist> Waitlist{ get; set; }
        public int temp { get; set; }

    }
}
