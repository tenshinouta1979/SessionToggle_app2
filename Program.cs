// Program.cs (for App2)
using System.Net.Http; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Register HttpClient for dependency injection
builder.Services.AddHttpClient(); // Registers the default HttpClient

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // Redirects HTTP to HTTPS
app.UseStaticFiles();     // Enables serving static files like CSS, JS, images

app.UseRouting();         // Enables routing for endpoints

app.UseAuthorization();   // If you had authentication/authorization policies

app.MapRazorPages();      // Maps incoming requests to Razor Pages

app.Run();                // Runs the application
