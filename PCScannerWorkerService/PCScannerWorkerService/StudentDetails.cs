using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCScannerWorkerService
{
    public class StudentDetails
    {
        public string Voornaam { get; set; }
        public string Naam { get; set; }
        public string Gebruikersnaam { get; set; }
        public List<Group> Groups { get; set; }
        public string Status { get; set; }
        public string Internnummer { get; set; }
    }
}
