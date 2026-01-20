namespace Volunteer.Models;

public class SupabaseSettings
{
    public string Url { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;
    public string CreateVolunteerUrl { get; set; } = string.Empty;
    public string EmailLinkUrl { get; set; } = string.Empty;
    public string UpdateInterestsUrl { get; set; } = string.Empty;
    public string UpdateVolunteerUrl { get; set; } = string.Empty;
    public string CheckVolunteerUrl { get; set; } = string.Empty;
}
