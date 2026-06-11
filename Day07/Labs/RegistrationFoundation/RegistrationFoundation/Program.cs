var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseStaticFiles();    // Serves .html .css .js from wwwroot
app.UseDefaultFiles();   // index.html loads at /


app.Run();
