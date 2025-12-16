using ExpenseTracker.Application;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Persistence;
using ExpenseTracker.Infrastructure.Auth;
using ExpenseTracker.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddFacades()
    .AddAuth(builder.Configuration);   

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/login.html");
app.Run();

