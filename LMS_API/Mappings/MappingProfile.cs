using AutoMapper;
using LMS_API.Models;
using LMS_API.Models.DTO;

namespace LMS_API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TeacherCreateDTO, Teacher>();
        }
    }
}
