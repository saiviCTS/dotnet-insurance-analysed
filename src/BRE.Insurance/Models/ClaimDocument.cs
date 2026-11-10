namespace BRE.Insurance.Models;

public class ClaimDocument
{
    public long Id { get; set; }
    public long ClaimId { get; set; }
    public Claim? Claim { get; set; }
    public ClaimDocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}

public enum ClaimDocumentType
{
    IncidentReport, MedicalBill, DamageReceipt,
    PoliceFir, HospitalDischargeSummary, Other
}
