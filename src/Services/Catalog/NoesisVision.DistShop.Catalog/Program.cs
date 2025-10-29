using Microsoft.EntityFrameworkCore;
using NoesisVision.DistShop.Catalog.Infrastructure.Data;
using NoesisVision.DistShop.Catalog.Domain.Repositories;
using NoesisVision.DistShop.Catalog.Infrastructure.Repositories;
using NoesisVision.DistShop.Catalog.Application.Services;
using NoesisVision.DistShop.SharedKernel.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(localdb)\\mssqllocaldb;Database=DistShop.Catalog;Trusted_Connection=true;MultipleActiveResultSets=true"));

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CatalogDbContext>());

// Register application services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

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

app.Run();