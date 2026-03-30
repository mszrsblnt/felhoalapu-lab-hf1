using CloudGalleryApi.Context;
using CloudGalleryApi.Setup;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<FormOptions>(options =>
{
    options.KeyLengthLimit = 4096;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});
builder.Services.AddCustomOpenApi();
builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>().AddEntityFrameworkStores<DataContext>();
builder.Services.AddDataProtection().
    PersistKeysToDbContext<DataContext>();

var frontendUrl = builder.Configuration["FrontendUrl"] ?? "";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(frontendUrl)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddScoped<CloudGalleryApi.Services.GalleryService>();
builder.Services.AddScoped<CloudGalleryApi.Services.PhotoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CloudGallery API v1 (OpenAPI)");
        options.RoutePrefix = "swagger";
    });
//}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapControllers();
app.MapIdentityApi<IdentityUser>();

// Migrate the database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate();
}

app.Run();
