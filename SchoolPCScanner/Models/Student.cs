using System.ComponentModel.DataAnnotations;

namespace SchoolPCScanner.Models
{
    public class Student
    {
        public int Id { get; set; }
        [Display(Name = "Voornaam")]
        public string Firstname { get; set; }
        [Display(Name = "Naam")]
        public string Lastname { get; set; }
        public string? Email { get; set; }
        [Display(Name = "Klas")]
        public string Grade { get; set; }
        [Display(Name = "Status leerling")]
        public bool IsActive { get; set; } // geeft aan of de leerling effectief op school zit (status van webapp)
        [Display(Name = "Smartschool Status")]
        public string Status { get; set; } // geeft aan of de leerling actief is of niet (status van Smartschool)

        public string Internnumber { get; set; }
        
        [Display(Name = "Geboortedatum")]
        public DateTime Birthdate { get; set; }
        [Display(Name = "GSM-nummer")]
        public string? Phonenumber { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
        [Display(Name = "Leerling")]
        public string FullName => $"{Firstname} {Lastname}";
        [Display(Name = "Nieuw leerling")]
        public bool IsNew { get; set; } // geeft aan of de leerling nieuw is
        public bool IsDeleted { get; set; } // geeft aan of de leerling verstopt is
        public DateTime DateAdded { get; set; } // datum wanneer de leerling is toegevoegd

        

    }
}
