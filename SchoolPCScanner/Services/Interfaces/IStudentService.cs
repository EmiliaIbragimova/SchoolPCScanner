using SchoolPCScanner.Models;

namespace SchoolPCScanner.Services.Interfaces
{
    public interface IStudentService
    {
        Task CreateStudentAsync(Student student);
        Task<IQueryable<Student>> GetAllStudentsAsync();
        Task<Student> GetStudentByIdAsync(int? id);
        Task<Student> GetStudentByGradeAsync(string grade);
        Task UpdateStudentAsync(Student student);
        Task DeleteStudentAsync(Student student);
        Task<bool> IsStudentUsedInTerminationAsync(Student student);
    }
}
