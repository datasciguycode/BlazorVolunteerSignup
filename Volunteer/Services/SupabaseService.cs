using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volunteer.Models;

namespace Volunteer.Services;

public class SupabaseService : ISupabaseService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SupabaseService> _logger;

    public SupabaseService(
        HttpClient httpClient,
        IOptions<SupabaseSettings> settings,
        IOptions<EmailSettings> emailSettings,
        ILogger<SupabaseService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    // -------------------------------------------------------------------------------------------------

    public async Task<(bool Success, string ErrorMessage)> SendVolunteerMessageAsync(VolunteerMessage message, string authToken)
    {
        try
        {
            _logger.LogInformation("Creating volunteer profile via create-volunteer-email-link Edge Function");

            // Transform the volunteer message into the format expected by create-volunteer-email-link function
            // Using snake_case property names to match the Supabase Edge Function
            var volunteerPayload = new
            {
                first_name = message.FirstName,
                last_name = message.LastName,
                phone_number = message.PhoneNumber,
                zip = message.Zip,
                body = message.Body
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _settings.CreateVolunteerUrl)
            {
                Content = JsonContent.Create(volunteerPayload, options: jsonOptions)
            };

            request.Headers.Add("Authorization", $"Bearer {authToken}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Volunteer profile created successfully");
                return (true, string.Empty);
            }

            // Handle 409 - Profile already exists
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.LogWarning("Profile already exists for this user");
                return (false, "A profile already exists for this account.");
            }

            // Handle 400 - Bad request (missing required fields)
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Bad request: Missing required fields. Error: {Error}", errorContent);
                return (false, "Please ensure all required fields are filled out correctly.");
            }

            // Handle other errors
            var generalErrorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create profile. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, generalErrorContent);

            return (false, "Unable to submit your information at this time. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating volunteer profile");
            return (false, "Unable to submit your information at this time. Please try again later.");
        }
    }

    // -------------------------------------------------------------------------------------------------

    public async Task<(bool Success, string ErrorMessage)> SendSignupEmailAsync(string email, string redirectUrl, string body = "")
    {
        try
        {
            _logger.LogInformation("Sending signup email request to Supabase Edge Function");

            var payload = new
            {
                to = email,
                redirectTo = redirectUrl,
                body = body
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_settings.EmailLinkUrl))
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload, jsonOptions),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            // Add Supabase API key header for authentication
            if (!string.IsNullOrEmpty(_settings.AnonKey))
            {
                request.Headers.Add("apikey", _settings.AnonKey);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Signup email sent successfully to {Email}", email);
                return (true, string.Empty);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send signup email. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return (false, "Unable to send signup email at this time. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending signup email");
            return (false, "Unable to send signup email at this time. Please try again later.");
        }
    }

    // -------------------------------------------------------------------------------------------------

    public async Task<(bool Success, string ErrorMessage)> UpdateVolunteerInterestsAsync(string email, List<long> interestIds, string authToken)
    {
        try
        {
            _logger.LogInformation("Updating volunteer interests for {Email}", email);

            var payload = new
            {
                Email = email,
                InterestIds = interestIds
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null // Use PascalCase as-is, don't convert to camelCase
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _settings.UpdateInterestsUrl)
            {
                Content = JsonContent.Create(payload, options: jsonOptions)
            };

            request.Headers.Add("Authorization", $"Bearer {authToken}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Volunteer interests updated successfully");
                return (true, string.Empty);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update volunteer interests. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return (false, $"Status {response.StatusCode}: {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while updating volunteer interests");
            return (false, $"Exception: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------------------------------

    public async Task<(bool Success, string ErrorMessage)> UpdateVolunteerAsync(string email, string aboutMyself, string emergencyContact, string authToken)
    {
        try
        {
            _logger.LogInformation("Updating volunteer info for {Email}", email);

            var payload = new
            {
                Email = email,
                AboutMyself = aboutMyself,
                EmergencyContact = emergencyContact
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null // Use PascalCase as-is, don't convert to camelCase
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _settings.UpdateVolunteerUrl)
            {
                Content = JsonContent.Create(payload, options: jsonOptions)
            };

            request.Headers.Add("Authorization", $"Bearer {authToken}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Volunteer info updated successfully");
                return (true, string.Empty);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update volunteer info. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return (false, $"Status {response.StatusCode}: {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while updating volunteer info");
            return (false, $"Exception: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------------------------------

    public async Task<List<Interest>> GetInterestsAsync(string? authToken = null)
    {
        try
        {
            _logger.LogInformation("Fetching interests from Supabase");

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_settings.Url}/rest/v1/interest?select=id,interest,interest_type_id,order_by&interest_type_id=eq.2&order=order_by.asc");

            request.Headers.Add("apikey", _settings.AnonKey);
            request.Headers.Add("Accept", "application/json");

            // Add Authorization header if auth token is provided
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Add("Authorization", $"Bearer {authToken}");
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var interests = await response.Content.ReadFromJsonAsync<List<Interest>>();
                _logger.LogInformation("Successfully fetched {Count} interests", interests?.Count ?? 0);
                return interests ?? new List<Interest>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to fetch interests. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new List<Interest>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while fetching interests from Supabase");
            return new List<Interest>();
        }
    }

    // -------------------------------------------------------------------------------------------------

    public async Task<List<Interest>> GetOutreachSubCommitteeAsync(string? authToken = null)
    {
        try
        {
            _logger.LogInformation("Fetching outreach sub-committee items from Supabase");

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_settings.Url}/rest/v1/interest?select=id,interest,interest_type_id,order_by&interest_type_id=eq.3&order=order_by.asc");

            request.Headers.Add("apikey", _settings.AnonKey);
            request.Headers.Add("Accept", "application/json");

            // Add Authorization header if auth token is provided
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Add("Authorization", $"Bearer {authToken}");
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<Interest>>();
                _logger.LogInformation("Successfully fetched {Count} outreach sub-committee items", items?.Count ?? 0);
                return items ?? new List<Interest>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to fetch outreach sub-committee items. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new List<Interest>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while fetching outreach sub-committee items from Supabase");
            return new List<Interest>();
        }
    }

    // -------------------------------------------------------------------------------------------------

    public async Task<List<Interest>> GetStandingCommitteeAsync(string? authToken = null)
    {
        try
        {
            _logger.LogInformation("Fetching standing committee items from Supabase");

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_settings.Url}/rest/v1/interest?select=id,interest,interest_type_id,order_by&interest_type_id=eq.4&order=order_by.asc");

            request.Headers.Add("apikey", _settings.AnonKey);
            request.Headers.Add("Accept", "application/json");

            // Add Authorization header if auth token is provided
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Add("Authorization", $"Bearer {authToken}");
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<Interest>>();
                _logger.LogInformation("Successfully fetched {Count} standing committee items", items?.Count ?? 0);
                return items ?? new List<Interest>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to fetch standing committee items. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new List<Interest>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while fetching standing committee items from Supabase");
            return new List<Interest>();
        }
    }

    // -------------------------------------------------------------------------------------------------

    public async Task<List<Interest>> GetLanguagesAsync(string? authToken = null)
    {
        try
        {
            _logger.LogInformation("Fetching languages from Supabase");

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_settings.Url}/rest/v1/interest?select=id,interest,interest_type_id,order_by&interest_type_id=eq.5&order=order_by.asc");

            request.Headers.Add("apikey", _settings.AnonKey);
            request.Headers.Add("Accept", "application/json");

            // Add Authorization header if auth token is provided
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Add("Authorization", $"Bearer {authToken}");
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<Interest>>();
                _logger.LogInformation("Successfully fetched {Count} languages", items?.Count ?? 0);
                return items ?? new List<Interest>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to fetch languages. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            return new List<Interest>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while fetching languages from Supabase");
            return new List<Interest>();
        }
    }

}
