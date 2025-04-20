namespace HMS.Models;

// To represent a visit record
public class VisitRecord
{
    public long Id { get; set; }
    public int PatientId { get; set; }
    public DateTime VisitDate { get; set; }
    public string Symptoms { get; set; }
    public string Examinations { get; set; }
    public string DiseaseType { get; set; }
    public string Diagnosis { get; set; }
    public int DoctorId { get; set; }
    public string Prescription { get; set; } 
    public decimal? TotalAmount { get; set; }
    public string Status { get; set; }
    public List<Prescription> Prescriptions { get; set; } = new List<Prescription>(); // For inserting/editing prescriptions
}