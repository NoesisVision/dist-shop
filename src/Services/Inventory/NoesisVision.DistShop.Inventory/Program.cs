using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.SharedKernel.Events;
using NoesisVision.DistShop.SharedKernel.Repositories;
using NoesisVision.DistShop.SharedKernel.Extensions;
using NoesisVision.DistShop.Inventory.Infrastructure.Data;
using NoesisVision.DistShop.Inventory.Infrastructure.Repositories;
using NoesisVision.DistShop.Inventory.Domain.Repositories;
using NoesisVision.DistShop.Inventory.Application.Services;
using NoesisVision.DistShop.Inventory.Application.EventHandlers;
using NoesisVision.DistShop.Catalog.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        "Server=(localdb)\\mssqllocaldb;Database=DistShop.Inventory;Trusted_Connection=true;MultipleActiveResultSets=true"));

// Register shared kernel services
builder.Services.AddSharedKernel<InventoryDbContext>();

// Register repositories
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

// Register application services
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Register event handlers
builder.Services.AddScoped<ProductCreatedEventHandler>();

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

// Subscribe to events
var eventBus = app.Services.GetRequiredService<IEventBus>();
var serviceProvider = app.Services;

// Subscribe to ProductCreatedEvent
eventBus.Subscribe<ProductCreatedEvent>(async productCreatedEvent =>
{
    using var scope = serviceProvider.CreateScope();
    var handler = scope.ServiceProvider.GetRequiredService<ProductCreatedEventHandler>();
    await handler.HandleAsync(productCreatedEvent);
});

app.Run();