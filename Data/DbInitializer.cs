using Microsoft.EntityFrameworkCore;
using HospitalSystem.Models;

namespace HospitalSystem.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if there are any users. If yes, assume DB is seeded.
            if (context.Users.Any())
            {
                return;
            }

            // 1. Create Admin
            var admin = new User
            {
                Username = "admin",
                PasswordHash = "admin123",
                FullName = "System Administrator",
                Role = UserRole.Admin,
                Email = "admin@medical2026.local",
                Phone = "1-800-ADMIN-00"
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();

            // 2. Create Departments & Specializations
            var depts = new List<Department>
            {
                new Department { Name = "Cardiology", Description = "Heart and cardiovascular system diseases." },
                new Department { Name = "Neurology", Description = "Disorders of the nervous system." },
                new Department { Name = "Orthopedics", Description = "Conditions involving the musculoskeletal system." },
                new Department { Name = "Pediatrics", Description = "Medical care of infants, children, and adolescents." },
                new Department { Name = "Dermatology", Description = "Conditions related to skin, hair, and nails." }
            };
            context.Departments.AddRange(depts);
            await context.SaveChangesAsync();

            var specs = new List<Specialization>
            {
                new Specialization { Name = "General Cardiologist", DepartmentId = depts[0].Id },
                new Specialization { Name = "Interventional Cardiologist", DepartmentId = depts[0].Id },
                
                new Specialization { Name = "Clinical Neurologist", DepartmentId = depts[1].Id },
                new Specialization { Name = "Neurosurgeon", DepartmentId = depts[1].Id },
                
                new Specialization { Name = "Spine Surgeon", DepartmentId = depts[2].Id },
                new Specialization { Name = "Sports Medicine", DepartmentId = depts[2].Id },
                
                new Specialization { Name = "General Pediatrician", DepartmentId = depts[3].Id },
                
                new Specialization { Name = "Cosmetic Dermatologist", DepartmentId = depts[4].Id },
                new Specialization { Name = "Medical Dermatologist", DepartmentId = depts[4].Id }
            };
            context.Specializations.AddRange(specs);
            await context.SaveChangesAsync();

            // 3. Create Doctors (approx. 10 doctors)
            var docNames = new[] { "John Doe", "Sarah Smith", "Michael Chang", "Emily Davis", "Robert Wilson", "Linda Taylor", "David Anderson", "Jennifer Thomas", "William Jackson", "Mary White" };
            var random = new Random();
            var doctors = new List<User>();

            for (int i = 0; i < docNames.Length; i++)
            {
                var doc = new User
                {
                    Username = $"doctor{i+1}",
                    PasswordHash = "doc123",
                    FullName = docNames[i],
                    Role = UserRole.Doctor,
                    Email = $"dr.{docNames[i].Replace(" ", "").ToLower()}@medical2026.local",
                    Phone = $"555-010{i}"
                };
                doctors.Add(doc);
            }
            context.Users.AddRange(doctors);
            await context.SaveChangesAsync();

            var doctorProfiles = new List<DoctorProfile>();
            foreach (var doc in doctors)
            {
                var randomSpec = specs[random.Next(specs.Count)];
                var profile = new DoctorProfile
                {
                    UserId = doc.Id,
                    SpecializationId = randomSpec.Id,
                    ConsultationFee = random.Next(80, 250),
                    Bio = $"Board-certified specialist in {randomSpec.Name} with over {random.Next(5, 20)} years of clinical experience."
                };
                doctorProfiles.Add(profile);
            }
            context.DoctorProfiles.AddRange(doctorProfiles);
            await context.SaveChangesAsync();

            // 4. Create Patients (approx. 20 patients)
            var patientFirstNames = new[] { "James", "Patricia", "Richard", "Barbara", "Joseph", "Susan", "Thomas", "Jessica", "Charles", "Karen", "Christopher", "Lisa", "Daniel", "Nancy", "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra" };
            var patientLastNames = new[] { "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee" };
            var patients = new List<User>();
            
            for (int i = 0; i < 20; i++)
            {
                var fn = patientFirstNames[random.Next(patientFirstNames.Length)];
                var ln = patientLastNames[random.Next(patientLastNames.Length)];
                var pat = new User
                {
                    Username = $"patient{i+1}",
                    PasswordHash = "pat123",
                    FullName = $"{fn} {ln}",
                    Role = UserRole.Patient,
                    Email = $"{fn.ToLower()}.{ln.ToLower()}@example.com",
                    Phone = $"555-200{i}"
                };
                patients.Add(pat);
            }
            context.Users.AddRange(patients);
            await context.SaveChangesAsync();

            var patientProfiles = new List<PatientProfile>();
            var medicalHistories = new[] { "None.", "Hypertension, taking Lisinopril.", "Type 2 Diabetes, controlled.", "Asthma.", "Allergic to Penicillin.", "Frequent migraines.", "Previous knee surgery in 2018.", "High cholesterol.", "No known allergies. Generally healthy." };
            var genders = new[] { "Male", "Female" };

            foreach (var pat in patients)
            {
                var profile = new PatientProfile
                {
                    UserId = pat.Id,
                    DateOfBirth = DateTime.Today.AddYears(-random.Next(18, 70)).AddDays(-random.Next(1, 365)),
                    Gender = genders[random.Next(genders.Length)],
                    MedicalHistory = medicalHistories[random.Next(medicalHistories.Length)]
                };
                patientProfiles.Add(profile);
            }
            context.PatientProfiles.AddRange(patientProfiles);
            await context.SaveChangesAsync();

            // 5. Create Appointments
            var appointments = new List<Appointment>();
            var statuses = new[] { AppointmentStatus.Pending, AppointmentStatus.Confirmed, AppointmentStatus.Completed, AppointmentStatus.Cancelled };
            
            // Create about 40 random appointments
            for (int i = 0; i < 40; i++)
            {
                var randomPatient = patientProfiles[random.Next(patientProfiles.Count)];
                var randomDoctor = doctorProfiles[random.Next(doctorProfiles.Count)];
                
                // Random date between 10 days ago and 20 days in the future
                var daysOffset = random.Next(-10, 20);
                var apptDate = DateTime.Today.AddDays(daysOffset).AddHours(random.Next(9, 17)).AddMinutes(random.Next(0, 2) * 30);
                
                // Logic for status based on date
                AppointmentStatus status;
                if (daysOffset < 0) 
                {
                    status = random.Next(0, 5) == 0 ? AppointmentStatus.Cancelled : AppointmentStatus.Completed;
                }
                else
                {
                    status = random.Next(0, 2) == 0 ? AppointmentStatus.Pending : AppointmentStatus.Confirmed;
                }

                var appt = new Appointment
                {
                    PatientId = randomPatient.Id,
                    DoctorId = randomDoctor.Id,
                    AppointmentDate = apptDate,
                    Status = status,
                    Notes = random.Next(0, 3) == 0 ? "Patient requests a general checkup." : (random.Next(0, 2) == 0 ? "Follow-up consultation." : "Experiencing mild symptoms.")
                };
                appointments.Add(appt);
            }
            
            context.Appointments.AddRange(appointments);
            await context.SaveChangesAsync();
        }
    }
}