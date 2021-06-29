using System.Linq;
using API.DTOs;
using API.Entity;
using API.Extensions;
using AutoMapper;

namespace API.Data.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles(){
            CreateMap<AppUser, MemberDTO>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(
                    src => src.Photos.FirstOrDefault(x => x.IsMain).Url
                ))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => 
                    src.DateOfBirth.CalculateAge()
                )); 
            CreateMap<Photo, PhotoDTO>();
            CreateMap<MemberUpdateDTO, AppUser>();
        }
    }
}