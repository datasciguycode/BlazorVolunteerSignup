using Volunteer.Models;

namespace Volunteer.Services;

public interface ISupabaseService
{
    Task<(bool Success, string ErrorMessage)> SendVolunteerMessageAsync(VolunteerMessage message, string authToken);
    Task<(bool Success, string ErrorMessage)> SendSignupEmailAsync(string email, string redirectUrl, string body = "");
    Task<(bool Success, string ErrorMessage)> UpdateVolunteerInterestsAsync(string email, List<long> interestIds, string authToken);
    Task<(bool Success, string ErrorMessage)> UpdateVolunteerAsync(string email, string aboutMyself, string emergencyContact, string authToken);
    Task<List<Interest>> GetInterestsAsync(string? authToken = null);
    Task<List<Interest>> GetOutreachSubCommitteeAsync(string? authToken = null);
    Task<List<Interest>> GetStandingCommitteeAsync(string? authToken = null);
    Task<List<Interest>> GetLanguagesAsync(string? authToken = null);
}
