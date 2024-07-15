using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;

namespace SchoolPCScanner.Services
{
    public class StudentService : IStudentService
    {
        private readonly SchoolPCScannerDbContext _context;

        public StudentService(SchoolPCScannerDbContext context)
        {
            _context = context;
        }

        public async Task CreateStudentAsync(Student student)
        {
            try
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("StudentService > CreateStudentAsync: An error occurred while creating student", ex);
            }
        }

        public async Task<IQueryable<Student>> GetAllStudentsAsync()
        {
            try
            {
                return _context.Students.Where(s => !s.IsDeleted);
            }
            catch (Exception ex)
            {
                throw new Exception("StudentService > GetAllStudentsAsync: An error occurred while retrieving students", ex);
            }
        }

        public async Task<Student> GetStudentByIdAsync(int? id)
        {
            try
            {
                return await _context.Students.Where(s => !s.IsDeleted).FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("StudentService > GetStudentByIdAsync: An error occurred while retrieving student by id", ex);
            }
        }

        public async Task<Student> GetStudentByGradeAsync(string grade)
        {
            try
            {
                return await _context.Students.FirstOrDefaultAsync(s => s.Grade == grade && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                throw new Exception("StudentService > GetStudentByGradeAsync: An error occurred while retrieving student by grade", ex);
            }
        }

        public async Task UpdateStudentAsync(Student student)
        {
            try
            {
                _context.Students.Update(student);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("StudentService > UpdateStudentAsync: An error occurred while updating student", ex);
            }
        }

        public async Task DeleteStudentAsync(Student student)
        {
            try
            {
                student.IsDeleted = true;
                student.IsActive = false;
                _context.Students.Update(student);
                //_context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("StudentService > DeleteStudentAsync: An error occurred while deleting student", ex);
            }
        }

        public async Task<bool> IsStudentUsedInTerminationAsync(Student student)
        {
            try
            {
                var isUsed = await _context.TerminationRegistrations.AnyAsync(t => t.StudentId == student.Id && !t.Student.IsDeleted && !t.Device.IsDeleted);
                return isUsed;
            }
            catch (Exception ex)
            {
                throw new Exception("StudentService > IsStudentUsedInTerminationAsync: An error occurred while checking if student is used in termination", ex);
            }
        }

        
    }

}
