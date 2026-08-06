using LMS_API.Models;
using LMS_API.Models.DTO.StudyClass;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{
    [Route("api/studyclass")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class StudyClassController : ControllerBase
    {
        private readonly IStudyClassService _studyClassService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<StudyClassController> _logger;

        public StudyClassController(IStudyClassService studyClassService, ITokenService tokenService, ILogger<StudyClassController> logger)
        {
            _studyClassService = studyClassService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<StudyClassReadDTO>> Create(StudyClassCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                return Unauthorized("Missing or invalid teacher identity.");

            var result = await _studyClassService.CreateStudyClassAsync(dto, teacherId);

            _logger.LogInformation("Study class created teacher_id={TeacherId} class_id={ClassId}", teacherId, result.Id);

            return Ok(result);
        }

        [HttpGet("teacher")]
        public async Task<ActionResult<IEnumerable<StudyClassReadDTO>>> GetByTeacher()
        {
            if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                return Unauthorized("Missing or invalid teacher identity.");

            var result = await _studyClassService.GetStudyClassesByTeacherAsync(teacherId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<StudyClassReadDTO>> GetById(int id)
        {
            if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                return Unauthorized("Missing or invalid teacher identity.");

            var result = await _studyClassService.GetStudyClassByIdAsync(id, teacherId);
            if (result == null)
                return NotFound($"StudyClass with id {id} not found");

            return Ok(result);
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                return Unauthorized("Missing or invalid teacher identity.");

            var success = await _studyClassService.DeleteStudyClassAsync(id, teacherId);

            if (!success)
                return NotFound($"StudyClass with id {id} not found");

            _logger.LogInformation("Study class deleted teacher_id={TeacherId} class_id={ClassId}", teacherId, id);

            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult<StudyClassReadDTO>> AddStudentsToStudyClass(StudyClassSyncDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                return Unauthorized("Missing or invalid teacher identity.");

            try
            {
                var result = await _studyClassService.AddStudentsToStudyClassAsync(dto, teacherId);

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}