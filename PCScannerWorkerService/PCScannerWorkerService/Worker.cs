using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxyClient;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;

namespace PCScannerWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailService _emailService;
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IEmailService emailService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _emailService = emailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        _logger.LogInformation("Creating scope for services.");
                        
                        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolPCScannerDbContext>();
                        _logger.LogInformation("SchoolPCScannerDbContext obtained.");

                        var proxyClient = scope.ServiceProvider.GetRequiredService<V3PortClient>();
                        _logger.LogInformation("V3PortClient obtained.");


                        // Zet inactieve studenten als niet langer actief
                        await UpdateInactiveStudentsAsync(dbContext, stoppingToken);
                        _logger.LogInformation("UpdateInactiveStudentsAsync completed.");


                        // Verwerk klassen en studenten
                        await ProcessClassesAndStudentsAsync(dbContext, proxyClient, stoppingToken);
                        _logger.LogInformation("ProcessClassesAndStudentsAsync completed.");


                        // Update nieuwe studenten
                        await UpdateNewStudentsAsync(dbContext, stoppingToken);
                        _logger.LogInformation("UpdateNewStudentsAsync completed.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Er is een onverwachte fout opgetreden bij het starten van de worker service");
                    await _emailService.SendEmailAsync("mala@piso.be", "Probleem met het starten van service", "Er is een onverwachte fout opgetreden bij het starten van de worker service");
                }

                _logger.LogInformation("Worker completed iteration at: {time}", DateTimeOffset.Now);

                // Vertraag 1 min voordat de volgende iteratie begint
                //await Task.Delay(120000, stoppingToken);
                // Wacht tot het volgende uitvoeringstijdstip
                var interval = CalculateIntervalUntilNextExecution();
                await Task.Delay((int)interval, stoppingToken);
            }

            _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);

        }

        private double CalculateIntervalUntilNextExecution()
        {
            // Bereken het interval tot het volgende uitvoeringstijdstip 5 minuten vanaf nu
            //var now = DateTime.Now;
            //var nextExecutionTime = now.AddMinutes(5);
            //return (nextExecutionTime - now).TotalMilliseconds;
            // Bereken het interval tot het volgende uitvoeringstijdstip om 03:00 uur 's nachts
            var now = DateTime.Now;
            var nextExecutionTime = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0);
            if (now >= nextExecutionTime)
            {
                nextExecutionTime = nextExecutionTime.AddDays(1); // Volgende dag om 03:00 uur
            }
            return (nextExecutionTime - now).TotalMilliseconds;
        }

        private async Task ProcessClassesAndStudentsAsync(SchoolPCScannerDbContext context, V3PortClient v3PortClient, CancellationToken stoppingToken)
        {
            string accesscode = Environment.GetEnvironmentVariable("SmartschoolAccesscode", EnvironmentVariableTarget.Machine);

            // Controleer of de environment variable is ingesteld
            if (string.IsNullOrEmpty(accesscode))
            {
                _logger.LogError("De environment variable 'ACCESS_CODE' is niet ingesteld.");
                await _emailService.SendEmailAsync("mala@piso.be", "Probleem met de verbinding met Smartschool", "Het aanroepen van de API is mislukt vanwege een ontbrekende toegangscode.");
            }

            var codes = new List<string>();

            try
            {
                // Haal alle klassen op, foutafhandeling
                var classList = await ExecuteWithRetry(() => v3PortClient.getClassListJsonAsync(accesscode), 3, TimeSpan.FromSeconds(2));
                var classListObject = JsonConvert.DeserializeObject<JArray>(classList);

                if (classListObject == null)
                {
                    _logger.LogWarning("Geen klassen gevonden.");
                    return;
                }

                foreach (var c in classListObject)
                {
                    //if (c["code"].ToString() == "1A1")
                    //{
                    codes.Add(c["code"].ToString());
                    Console.WriteLine("Name: " + c["name"]);
                    //}
                    //else
                    //{
                    //    Console.WriteLine("These students are already added.");
                    //}
                }

                //loop door de lijst van accounts en haal leerlinggegevens op voor elke klas
                foreach (var code in codes)
                {
                    // haal de lijst van accounts op, foutafhalndelin
                    var allAccounts = await ExecuteWithRetry(() => v3PortClient.getAllAccountsExtendedAsync(accesscode, code, "0"), 3, TimeSpan.FromSeconds(2));

                    //Console.WriteLine("Accounts: " + allAccounts);
                    var studentDetailsList = JsonConvert.DeserializeObject<List<StudentDetails>>((string)allAccounts);

                    if (studentDetailsList != null)
                    {
                        // Loop door elke StudentDetails in de lijst en behandel ze afzonderlijk
                        foreach (var studentDetails in studentDetailsList)
                        {
                            // Converteer de StudentDetails naar een Studentobject
                            Student student = StudentConverter.ConvertToStudent(studentDetails);

                            var existingStudent = await context.Students.FirstOrDefaultAsync(s => s.Internnumber == student.Internnumber);

                            if (existingStudent == null)
                            {
                                if (student.Status == "actief")
                                {
                                    student.IsActive = true;
                                    student.IsNew = true;
                                    student.DateAdded = DateTime.Now;
                                    context.Students.Add(student);
                                    _logger.LogInformation($"Nieuwe student toegevoegd: {student.Firstname} {student.Lastname}, Klas: {student.Grade}");
                                    await _emailService.SendEmailAsync("mala@piso.be", "Nieuwe student toegevoegd", $"Nieuwe student toegevoegd: {student.Firstname} {student.Lastname}");

                                    Console.WriteLine($"Voornaam: {student.Firstname}");
                                    Console.WriteLine($"Achternaam: {student.Lastname}");
                                    Console.WriteLine($"Internnummer: {student.Internnumber}");
                                    Console.WriteLine($"Status: {student.Status}");
                                    Console.WriteLine($"Klas: {student.Grade}");
                                    Console.WriteLine($"Actief: {student.IsActive}");
                                    Console.WriteLine($"Nieuw: {student.IsNew}");
                                    Console.WriteLine($"Datum toegevoegd: {student.DateAdded}");
                                }

                            }
                            else // Als er een overeenkomende student is gevonden, werk dan de gegevens bij
                            {
                                existingStudent.Firstname = student.Firstname;
                                existingStudent.Lastname = student.Lastname;
                                existingStudent.Internnumber = student.Internnumber;
                                existingStudent.Status = student.Status;
                                existingStudent.Grade = student.Grade;
                                existingStudent.IsDeleted = false;
                                _logger.LogInformation($"Student bestaat al: {student.Firstname} {student.Lastname}");
                            }
                        }
                    }

                    else
                    {
                        _logger.LogWarning($"Kon geen accounts ophalen voor code {string.Join(", ", code)}. Controleer of de klas nog bestaat of de juiste naam heeft.");
                        await _emailService.SendEmailAsync("mala@piso.be", "Probleem met klassen", $"Kon geen accounts ophalen voor code {string.Join(", ", code)}. Controleer of de klas nog bestaat of de juiste naam heeft.");
                    }

                }
                await context.SaveChangesAsync(stoppingToken); // Opslaan van de wijzigingen in de database
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij het verwerken van klassen en studenten.");
                await _emailService.SendEmailAsync("mala@piso.be", "Fout bij het verwerken van klassen en studenten", $"Fout bij het verwerken van accounts voor code {string.Join(", ", codes)}: {ex.Message}");
            }
        }


        private async Task UpdateNewStudentsAsync(SchoolPCScannerDbContext context, CancellationToken stoppingToken)
        {
            var timeAgo = DateTime.Now.AddDays(-7); // Bepaal de datum van 7 dagen geleden

            // Haal alle nieuwe studenten op die 7 dagen geleden zijn toegevoegd

            //var timeAgo = DateTime.Now.AddMinutes(-15);
            //var timeAgo = DateTime.Now.AddHours(-2);
            var newStudents = new List<Student>();

            try
            {

                newStudents = context.Students.Where(s => s.IsNew && s.DateAdded < timeAgo).ToList();

                foreach (var student in newStudents)
                {
                    // Stel de student in als niet nieuw
                    student.IsNew = false;
                    _logger.LogInformation($"Student {student.Firstname} {student.Lastname} is niet langer nieuw.");
                }
                await context.SaveChangesAsync(stoppingToken); // Opslaan van de wijzigingen in de database

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fout bij het bijwerken van de status voor student");
                await _emailService.SendEmailAsync("mala@piso.be", "Probleem bij het bijwerken van studentstatus", $"Er is een fout opgetreden bij het bijwerken van de status voor student.");
            }
        }



        // methode om inactieve studenten te inactive te zetten
        private async Task UpdateInactiveStudentsAsync(SchoolPCScannerDbContext context, CancellationToken stoppingToken)
        {
            var inactiveStudents = await context.Students
        .Where(s => s.Status == "uitgeschakeld")
        .ToListAsync();

            //try
            //{
            if (inactiveStudents.Any())
            {

                foreach (var student in inactiveStudents)
                {
                    var devices = await context.Devices.Where(d => d.StudentId == student.Id).ToListAsync();

                    if (!devices.Any())
                    {
                        try
                        {
                            // Er zijn geen apparaten gekoppeld aan deze student
                            student.IsActive = false;
                            _logger.LogInformation($"Student {student.Firstname} {student.Lastname} is niet langer actief.");
                        }
                        catch (Exception ex)
                        {
                            // Er zijn apparaten gekoppeld aan deze student
                            _logger.LogInformation($"Student {student.Firstname} {student.Lastname} is gekoppeld aan een toestel en kan nog niet uitgeschreven worden.");
                            await _emailService.SendEmailAsync("mala@piso.be", "Student kan niet uitgeschreven worden", $"Student {student.Firstname} {student.Lastname} is gekoppeld aan een toestel en kan nog niet uitgeschreven worden. De student moet dringend de laptop binnenbrengen.");
                        }
                    }
                }
                await context.SaveChangesAsync(stoppingToken);
            }

        }

        public async Task<T> ExecuteWithRetry<T>(Func<Task<T>> apiCall, int retryCount, TimeSpan delay)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    return await apiCall();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"API-aanroep mislukt. Poging {i + 1}/{retryCount}");
                    await Task.Delay(delay);
                }
            }

            _logger.LogInformation("API-aanroep mislukt na meerdere pogingen.");
            await _emailService.SendEmailAsync("mala@piso.be", "Service problem", "API-aanroep mislukt na meerdere pogingen.");
            // Als de methode na het aantal pogingen nog steeds mislukt, gooi een uitzondering of geef een foutmelding terug.
            throw new Exception("API-aanroep mislukt na meerdere pogingen.");


        }
    }
}
