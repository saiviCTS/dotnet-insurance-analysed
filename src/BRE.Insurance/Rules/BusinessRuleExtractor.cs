using BRE.Insurance.Constants;
using System.Reflection;

namespace BRE.Insurance.Rules;

/// <summary>
/// Business Rule Extractor (.NET)
/// ─────────────────────────────────────────────────────────────
/// Uses .NET reflection to scan BusinessRuleConstants and produce
/// a machine-readable catalog of all embedded business rules.
/// Output is consumed by AWS Transform analysis scripts.
/// </summary>
public class BusinessRuleExtractor
{
    public record BusinessRule(string RuleId, string Description, string Value, string Module);

    /// <summary>Extracts all rules from BusinessRuleConstants via reflection.</summary>
    public List<BusinessRule> ExtractAllRules()
    {
        var rules = new List<BusinessRule>();

        foreach (var field in typeof(BusinessRuleConstants).GetFields(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
        {
            if (!field.IsLiteral) continue;

            string name   = field.Name;
            string value  = field.GetRawConstantValue()?.ToString() ?? "N/A";
            string module = DeriveModule(name);
            string ruleId = DeriveRuleId(name);
            string desc   = Humanize(name);

            rules.Add(new BusinessRule(ruleId, desc, value, module));
        }
        return rules;
    }

    public Dictionary<string, List<BusinessRule>> ExtractByModule()
    {
        var byModule = new Dictionary<string, List<BusinessRule>>();
        foreach (var rule in ExtractAllRules())
        {
            if (!byModule.ContainsKey(rule.Module))
                byModule[rule.Module] = new List<BusinessRule>();
            byModule[rule.Module].Add(rule);
        }
        return byModule;
    }

    public void PrintRulesCatalog()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        INSURANCE BUSINESS RULES CATALOG (.NET 8)            ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

        foreach (var (module, rules) in ExtractByModule())
        {
            Console.WriteLine($"\n── {module} ──────────────────────────────");
            foreach (var r in rules)
                Console.WriteLine($"  {r.RuleId,-20} {r.Description,-50} = {r.Value}");
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string DeriveModule(string name) => name switch
    {
        var n when n.StartsWith("MinPolicyholder") || n.StartsWith("MaxLife")
               || n.StartsWith("Kyc")             => "KYC & Registration",
        var n when n.Contains("Property") || n.Contains("Vehicle")
               || n.Contains("Vin")               => "Asset & Property",
        var n when n.StartsWith("SumAssured") || n.StartsWith("PreExisting")
               || n.StartsWith("MedicalExam") || n.StartsWith("MaxDependents")
               || n.StartsWith("MinSumAssured") || n.StartsWith("Smoker")
                                                   => "Life & Health",
        var n when n.StartsWith("Corporate")
               || n.StartsWith("FamilyFloater")    => "Insurance Category",
        var n when n.StartsWith("MinPremium") || n.StartsWith("MaxSingle")
               || n.StartsWith("MaxPayment") || n.StartsWith("Payment")
               || n.StartsWith("CreditCard")       => "Payment Gateway",
        var n when n.StartsWith("Sip")             => "SIP",
        var n when n.StartsWith("Renewal") || n.StartsWith("ClaimStatus")
               || n.StartsWith("MaxSms")           => "Notifications",
        var n when n.StartsWith("CoolingOff")
               || n.StartsWith("Cancellation")     => "Cancellation & Refund",
        var n when n.StartsWith("ClaimFiling") || n.StartsWith("PolicyWaiting")
               || n.StartsWith("ClaimMax") || n.StartsWith("ClaimMin")
                                                   => "Claim Filing",
        var n when n.StartsWith("Surveyor") || n.StartsWith("ClaimDecision")
               || n.StartsWith("Disbursement")     => "Claim Tracking",
        var n when n.StartsWith("Helpdesk") || n.StartsWith("Callback")
               || n.StartsWith("MaxOpen")          => "Helpdesk",
        var n when n.StartsWith("Feedback")        => "Feedback & Ratings",
        _                                          => "General"
    };

    private static string DeriveRuleId(string name) => name switch
    {
        var n when n.StartsWith("MinPolicyholder") || n.StartsWith("MaxLife")
               || n.StartsWith("Kyc")     => "BR-KYC",
        var n when n.Contains("Property") || n.Contains("Vehicle")
               || n.Contains("Vin")       => "BR-ASSET",
        var n when n.StartsWith("SumAssured") || n.StartsWith("PreExisting")
               || n.StartsWith("Medical") || n.StartsWith("MaxDependents")
               || n.StartsWith("MinSumAssured") || n.StartsWith("Smoker")
                                          => "BR-LIFE",
        var n when n.StartsWith("Corporate")
               || n.StartsWith("FamilyFloater") => "BR-CAT",
        var n when n.StartsWith("MinPremium") || n.StartsWith("MaxSingle")
               || n.StartsWith("MaxPayment") || n.StartsWith("PaymentSession")
               || n.StartsWith("CreditCard")  => "BR-PAY",
        var n when n.StartsWith("Sip")    => "BR-SIP",
        var n when n.StartsWith("Renewal") || n.StartsWith("ClaimStatus")
               || n.StartsWith("MaxSms") => "BR-NOTIF",
        var n when n.StartsWith("CoolingOff")
               || n.StartsWith("Cancellation") => "BR-CANCEL",
        var n when n.StartsWith("ClaimFiling") || n.StartsWith("PolicyWaiting")
               || n.StartsWith("ClaimMax") || n.StartsWith("ClaimMin") => "BR-CLAIM",
        var n when n.StartsWith("Surveyor") || n.StartsWith("ClaimDecision")
               || n.StartsWith("Disbursement") => "BR-TRACK",
        var n when n.StartsWith("Helpdesk") || n.StartsWith("Callback")
               || n.StartsWith("MaxOpen") => "BR-HELP",
        var n when n.StartsWith("Feedback") => "BR-FEEDBACK",
        _ => "BR-GEN"
    };

    private static string Humanize(string name)
    {
        // PascalCase → "Pascal Case"
        var result = System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim();
        return result.ToLower();
    }
}
