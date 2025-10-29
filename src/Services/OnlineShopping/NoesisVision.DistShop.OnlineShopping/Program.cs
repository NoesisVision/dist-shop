using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.SharedKernel.Extensions;
using NoesisVision.DistShop.OnlineShopping.Infrastructure.Data;
using NoesisVision.DistShop.OnlineShopping.Domain.Repositories;
using NoesisVision.DistShop.OnlineShopping.Infrastructure.Repositories;
using NoesisVision.DistShop.OnlineShopping.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Online Shopping Service API", 
        Version = "v1",
        Description = "API for managing shopping carts and checkout operations"
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Entity Framework
builder.Services.AddDbContext<OnlineShoppingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        "Server=(localdb)\\mssqllocaldb;Database=DistShop.OnlineShopping;Trusted_Connection=true;MultipleActiveResultSets=true"));

// Add shared kernel services (event bus, unit of work, etc.)
builder.Services.AddSharedKernel<OnlineShoppingDbContext>();

// Add domain repositories
builder.Services.AddScoped<ICartRepository, CartRepository>();

// Add integration services
builder.Services.AddScoped<IPricingIntegrationService, PricingIntegrationService>();
builder.Services.AddScoped<IInventoryIntegrationService, InventoryIntegrationService>();
builder.Services.AddScoped<IOrderIntegrationService, OrderIntegrationService>();

// Add application services
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

// Add logging
builder.Services.AddLogging();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Shopping Service API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OnlineShoppingDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not ensure database is created. This is expected in some environments.");
    }
}

app.Run();