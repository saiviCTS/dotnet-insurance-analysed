namespace BRE.Insurance.Models;

public class KycDocument
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public DocumentType DocumentType { get; set; }
    public DocumentCategory DocumentCategory { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public bool Verified { get; set; }
}

public enum DocumentType
{
    Passport, NationalId, DriversLicense,
    UtilityBill, BankStatement,
    SalarySlip, OfferLetter
}

public enum DocumentCategory { ProofOfIdentity, ProofOfAddress, ProofOfEmployment }
