using AutoMapper;
using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Models.DTO.Assignmentset;
using LMS_API.Models.DTO.Student;
using LMS_API.Models.DTO.Teacher;


namespace LMS_API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TeacherCreateDTO, Teacher>();
            CreateMap<Assignment, AssignmentCreateDTO>().ReverseMap();
            CreateMap<Assignment, AssignmentReadDTO>().ReverseMap();
            CreateMap<AssignmentSet, AssignmentSetCreateDTO>().ReverseMap();
            CreateMap<StudentCreateDTO, Student>();
            CreateMap<AssignmentSet, AssignmentSetReadDTO>()
                .ForMember(dest => dest.Assignments,
                    opt => opt.MapFrom(src => src.AssignmentAssignmentSets.Select(x => x.Assignment)));
        }
    }
}
