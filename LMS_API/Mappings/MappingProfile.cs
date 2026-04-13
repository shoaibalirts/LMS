using AutoMapper;
using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Models.DTO.AssignmentSet;
using LMS_API.Models.DTO.Student;
using LMS_API.Models.DTO.Teacher;
using LMS_API.Models.DTO.StudyClass;

namespace LMS_API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TeacherCreateDTO, Teacher>();

            CreateMap<Assignment, AssignmentCreateDTO>().ReverseMap();
            CreateMap<Assignment, AssignmentReadDTO>().ReverseMap();

            CreateMap<StudentCreateDTO, Student>();
            CreateMap<Student, StudentReadDTO>();

            CreateMap<AssignmentSetCreateDTO, AssignmentSet>();
            CreateMap<AssignmentSet, AssignmentSetReadDTO>()
                .ForMember(dest => dest.Assignments,
                    opt => opt.MapFrom(src => src.AssignmentAssignmentSets.Select(x => x.Assignment)));

            CreateMap<StudyClass, StudyClassCreateDTO>();

            CreateMap<StudyClass, StudyClassReadDTO>()
                .ForMember(dest => dest.Students,
                    opt => opt.MapFrom(src => src.StudentStudyClasses.Select(x => x.Student)));
        }
    }
}