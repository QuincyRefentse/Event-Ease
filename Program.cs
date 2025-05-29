using BlobStorage.Services;
using EventEase.Data;
using EventEase.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Load the configuration
builder.Services.Configure<AzureBlobStorage>(builder.Configuration.GetSection("AzureBlobStorage"));

// Add services to the container
builder.Services.AddControllersWithViews();


// Add the ApplicationDbContext to the dependency injection container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add the services for initializing blob storage to the dependency injection container
builder.Services.Configure<AzureBlobStorage>(builder.Configuration.GetSection("AzureBlobStorage"));
builder.Services.AddSingleton<BlobService>();

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add custom route for Event/Create (optional)
app.MapControllerRoute(
    name: "Events",
    pattern: "events/index", // Custom route to EventController/Create
    defaults: new { controller = "Events", action = "create" });

// Add custom route for Event/Create (optional)
app.MapControllerRoute(
    name: "Bookings",
    pattern: "bookings/create", // Custom route to EventController/Create
    defaults: new { controller = "Bookings", action = "Create" });

// Add custom route for Event/Create (optional)
app.MapControllerRoute(
    name: "Venues",
    pattern: "venues/create", // Custom route to EventController/Create
    defaults: new { controller = "Venues", action = "Create" });

app.Run();
