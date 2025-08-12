using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json; // For PostAsJsonAsync
using System.Threading.Tasks;
using System;
using SessionToggleApp.Models; // Import App2's own models for validation request/response

namespace SessionToggleApp.Pages
{
    public class IndexModel : PageModel
    {
        // HttpClient will be injected for making API calls
        private readonly HttpClient _httpClient;

        // Constructor for dependency injection
        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Properties for UI display
        [BindProperty(SupportsGet = true)]
        public string ReceivedGuid { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string Data { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public bool IsLoading { get; set; } = false; // Fixed: Removed extra '=' here

        [BindProperty(SupportsGet = true)]
        public int App2ToApp1CallCount { get; set; } = 0;

        [BindProperty(SupportsGet = true)]
        public string StatusMessage { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string ReceivedApp1SessionId { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string App1ValidationUserName { get; set; } = "";


        public async Task OnGetAsync(string authGuid)
        {
            if (!string.IsNullOrEmpty(authGuid))
            {
                ReceivedGuid = authGuid;
                // On initial load, immediately attempt to validate GUID with App1's backend
                await SimulateDataFetch();
            }
            else
            {
                ReceivedGuid = "";
                Data = "No GUID received from App1. Please access App2 via App1.";
                ReceivedApp1SessionId = "";
                App1ValidationUserName = "";
            }
        }

        public async Task<IActionResult> OnPostPerformActionAsync()
        {
            // Always trigger an actual data fetch/validation call to App1.
            await SimulateDataFetch();
            
            // Retain the received GUID across the postback.
            string currentGuid = HttpContext.Request.Query["authGuid"].ToString();
            if (!string.IsNullOrEmpty(currentGuid))
            {
                ReceivedGuid = currentGuid;
            }

            return Page();
        }

        /// <summary>
        /// Makes an actual HTTP POST call to App1's API endpoint to validate the GUID.
        /// </summary>
        private async Task SimulateDataFetch()
        {
            IsLoading = true;
            Data = "";
            StatusMessage = "";
            ReceivedApp1SessionId = "";
            App1ValidationUserName = "";

            // Simulate a very short delay for App2's internal processing before making the external call
            await Task.Delay(new Random().Next(50, 100));

            if (string.IsNullOrEmpty(ReceivedGuid))
            {
                StatusMessage = "Cannot validate: No GUID available.";
                IsLoading = false;
                Data = "Please access App2 through App1 to receive an authentication GUID.";
                return;
            }

            try
            {
                // Define the URL for App1's GUID validation API endpoint
                // IMPORTANT: Replace with the actual URL of your App1's validation endpoint.
                // For local development, this is typically App1's port, e.g., http://localhost:XXXX/api/ValidateGuid
                // (Make sure to run App1 first and note its port, usually 7XXX or 5XXX for HTTP)
                string app1ValidationApiUrl = "http://localhost:5221/api/ValidateGuid"; // Adjust this URL based on your App1's actual port

                StatusMessage = $"Sending actual request to App1 ({app1ValidationApiUrl}) for validation... 📡";
                
                // Prepare the request payload
                var requestPayload = new ValidationRequest { GuidToValidate = ReceivedGuid };

                // Make the actual HTTP POST request
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(app1ValidationApiUrl, requestPayload);
                
                // Increment call count *after* the request is sent (even if it fails)
                App2ToApp1CallCount++;

                if (response.IsSuccessStatusCode)
                {
                    ValidationResponse validationResponse = await response.Content.ReadFromJsonAsync<ValidationResponse>();
                    if (validationResponse.IsValid)
                    {
                        ReceivedApp1SessionId = validationResponse.App1SessionId;
                        App1ValidationUserName = validationResponse.UserName;
                        StatusMessage = $"Callback received from App1! Valid. App1 Session ID: {ReceivedApp1SessionId} ✅";
                        Data = $"GIS content displayed for {App1ValidationUserName}. (Re-validated via App1) ⏳";
                    }
                    else
                    {
                        StatusMessage = $"Callback received from App1! Invalid. Message: {validationResponse.Message} ❌";
                        Data = "Could not display GIS content. Validation failed with App1.";
                    }
                }
                else
                {
                    // Handle HTTP error status codes
                    string errorContent = await response.Content.ReadAsStringAsync();
                    StatusMessage = $"Error: App1 API returned status {response.StatusCode}. Details: {errorContent} ❗";
                    Data = "Failed to communicate with App1 for validation.";
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle network errors (e.g., App1 not running, connection issues)
                StatusMessage = $"Network Error: Could not connect to App1. Is App1 running? {ex.Message} 🛑";
                Data = "Failed to connect to App1 for validation. Check App1 status.";
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                StatusMessage = $"An unexpected error occurred: {ex.Message} ⚠️";
                Data = "An unexpected error occurred during validation.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
