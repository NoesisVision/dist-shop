using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.SharedKernel.Extensions;
using NoesisVision.DistShop.Pricing.Infrastructure.Data;
using NoesisVision.DistShop.Pricing.Domain.Repositories;
using NoesisVision.DistShop.Pricing.Infrastructure.Repositories;
using NoesisVision.DistShop.Pricing.Application.Services;
using NoesisVision.DistShop.Pricing.Domain.Services;
using NoesisVision.DistShop.SharedKernel.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<PricingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        "Server=(localdb)\\mssqllocaldb;Database=DistShop.Pricing;Trusted_Connection=true;MultipleActiveResultSets=true"));

// Add shared kernel services
builder.Services.AddSharedKernel();

// Register repositories
builder.Services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PricingDbContext>());

// Register domain services
builder.Services.AddScoped<PricingEngine>();

// Register application services
builder.Services.AddScoped<IPriceCalculatorService, PriceCalculatorService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PricingDbContext>();
    context.Database.EnsureCreated();
}

app.Run();