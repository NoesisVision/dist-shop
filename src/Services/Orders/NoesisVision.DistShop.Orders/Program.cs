using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.SharedKernel.Extensions;
using NoesisVision.DistShop.Orders.Infrastructure.Data;
using NoesisVision.DistShop.Orders.Domain.Repositories;
using NoesisVision.DistShop.Orders.Infrastructure.Repositories;
using NoesisVision.DistShop.Orders.Application.Services;
using NoesisVision.DistShop.Orders.Application.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        "Server=(localdb)\\mssqllocaldb;Database=DistShop.Orders;Trusted_Connection=true;MultipleActiveResultSets=true"));

// Register shared kernel services (event bus, etc.)
builder.Services.AddSharedKernel();

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register application services
builder.Services.AddScoped<IOrderService, OrderService>();

// Register event handlers
builder.Services.AddScoped<StockReservedEventHandler>();
builder.Services.AddScoped<OrderStatusChangedEventHandler>();

// Add logging
builder.Services.AddLogging();

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
    var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    context.Database.EnsureCreated();
}

app.Run();