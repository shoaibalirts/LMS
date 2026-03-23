using LMS_API.Models;
namespace LMS_API.Repositories
{
    public interface ITaskRepository
    {
        Task<Models.Task> AddTaskAsync(Models.Task task);
    }
}