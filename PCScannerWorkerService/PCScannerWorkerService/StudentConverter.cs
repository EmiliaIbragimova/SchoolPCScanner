using SchoolPCScanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCScannerWorkerService
{
    public class StudentConverter
    {
        public static Student ConvertToStudent(StudentDetails studentDetails)
        {

            return new Student
            {
                Firstname = studentDetails.Voornaam,
                Lastname = studentDetails.Naam,
                Internnumber = studentDetails.Internnummer,
                Status = studentDetails.Status,
                Grade = studentDetails.Groups.FirstOrDefault()?.Name,
            };
        }
    }
}
