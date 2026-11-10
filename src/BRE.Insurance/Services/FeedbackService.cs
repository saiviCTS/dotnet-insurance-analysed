using BRE.Insurance.Constants;
using BRE.Insurance.Models;

namespace BRE.Insurance.Services;

/// <summary>
/// Feedback Service — Module 12: User Feedback &amp; Rating
///
/// Business Rules:
///   BR-FEEDBACK-001 : Feedback window = 30 days after disbursement
///   BR-FEEDBACK-002/003: Rating must be 1–5
///   BR-FEEDBACK-004 : Comment mandatory if rating &lt;= 2
///   BR-FEEDBACK-005 : Comment min 20 characters when mandatory
/// </summary>
public class FeedbackService
{
    public void ValidateFeedbackWindow(Claim claim)
    {
        if (claim.DisbursedAt is null)
            throw new InvalidOperationException(
                "BR-FEEDBACK-001 VIOLATED: Feedback only allowed after claim disbursement.");

        int daysSince = (DateTime.UtcNow - claim.DisbursedAt.Value).Days;
        if (daysSince > BusinessRuleConstants.FeedbackWindowDays)
            throw new InvalidOperationException(
                $"BR-FEEDBACK-001 VIOLATED: Feedback window of " +
                $"{BusinessRuleConstants.FeedbackWindowDays} days expired. Days: {daysSince}");
    }

    public void ValidateRating(int rating)
    {
        if (rating < BusinessRuleConstants.FeedbackMinRating ||
            rating > BusinessRuleConstants.FeedbackMaxRating)
            throw new ArgumentException(
                $"BR-FEEDBACK-002/003 VIOLATED: Rating must be " +
                $"{BusinessRuleConstants.FeedbackMinRating}–{BusinessRuleConstants.FeedbackMaxRating}. Got: {rating}");
    }

    public void ValidateComment(int rating, string? comment)
    {
        if (rating <= BusinessRuleConstants.FeedbackCommentMandatoryBelowRating)
        {
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException(
                    $"BR-FEEDBACK-004 VIOLATED: Comment is mandatory when rating <= " +
                    $"{BusinessRuleConstants.FeedbackCommentMandatoryBelowRating}.");

            if (comment.Trim().Length < BusinessRuleConstants.FeedbackMinCommentLength)
                throw new ArgumentException(
                    $"BR-FEEDBACK-005 VIOLATED: Comment must be at least " +
                    $"{BusinessRuleConstants.FeedbackMinCommentLength} chars.");
        }
    }

    public Feedback Submit(User user, Claim claim, int rating, string? comment)
    {
        ValidateFeedbackWindow(claim);
        ValidateRating(rating);
        ValidateComment(rating, comment);

        return new Feedback
        {
            User         = user,
            UserId       = user.Id,
            Claim        = claim,
            ClaimId      = claim.Id,
            Rating       = rating,
            Comment      = comment,
            FeedbackType = FeedbackType.PostClaim,
            SubmittedAt  = DateTime.UtcNow
        };
    }
}
