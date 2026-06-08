using CareBridge.EFCoreDemo.Models.Generated;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core DbContext.
// ASP.NET Core will automatically create and inject it when needed.
builder.Services.AddDbContext<CareBridgeScaffoldContext>();

// Add Swagger support.
// Swagger gives us a testing screen for APIs.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow Vue.js running on another port
// to call this API from the browser.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable Swagger.
app.UseSwagger();
app.UseSwaggerUI();

// Enable CORS.
app.UseCors();

// Simple health-check endpoint.
app.MapGet("/", () =>
{
    return "CareBridge API is running";
});

// Return first 20 patients.
// EF Core converts this LINQ query into SQL.
app.MapGet("/api/patients",
    (CareBridgeScaffoldContext db) =>
    {
        return db.Patients

                 // Select only columns we need.
                 .Select(p => new
                 {
                     p.PatientId,
                     p.FullName,
                     p.City
                 })

                 // Return only first 20 rows.
                 .Take(20)

                 // Execute query.
                 .ToList();
    });

app.MapGet("/api/patients/search",([FromQuery] string? city, [FromQuery] bool? activeOnly,[FromQuery] string? fullName, CareBridgeScaffoldContext db) =>
    {
        var query = db.Patients.AsQueryable();

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(p => p.City == city);
        }

        if (!string.IsNullOrEmpty(fullName))
        {
            query = query.Where(p => p.FullName.Contains(fullName));
        }

        if (activeOnly == false)
        {
            query = query.Where(p => p.IsActive);
        }

        return query
            .OrderBy(p => p.FullName) 
            .Select(p => new
            {
                p.PatientId,
                p.FullName,
                p.City,
                p.IsActive
            })

            .Take(20)

            .ToList();
    });


app.Run();