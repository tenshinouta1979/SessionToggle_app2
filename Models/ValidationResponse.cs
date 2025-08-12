// SessionToggleApp/Models/ValidationResponse.cs
namespace SessionToggleApp.Models
{
    public class ValidationResponse
    {
        public bool IsValid { get; set; }
        public string App1SessionId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}