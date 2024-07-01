using DevDiary.Data;
using DevDiary.Models;
using DevDiary.Services.Interfaces;
using DevDiary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


// Create a new web application builder with command line arguments
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration) ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


// Configure Entity Framwork to use PostgreSQL and set custom migration history table
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.MigrationsHistoryTable(tableName: "BlogMigrationHistory", schema: "blog")));

// Add a developer exception filter for database-related
// Helps handle database errors and provides useful information for troubleshooting during development
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


// Configure Identity services with custom user and role
builder.Services.AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();


// Register Custom Services with dependency injection
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IEmailSender, EmailService>();

// Bind the email settings to the EmailSettings object
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSetting"));

// Add MVC services to the container
 // More freedom to customize and edit
builder.Services.AddMvc();

// Add and configure Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Dev Diary",
        Version = "v1",
        Description = "An API accessible to the public that retrieves the most recent blog posts",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Gelson Hernandez",
            Email = "gelsonhz@outlook.com",
            Url = new Uri("https://gelsonportfolio.netlify.app/")
        }
    });
    // Get the XML comments file name
    string xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; 

     // Include XML comments for Swagger documentation
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});

// Add CORS policy to allow any origin, method, and header
builder.Services.AddCors(cors =>
{
    cors.AddPolicy("DefaultPolicy", builder => builder.AllowAnyOrigin()
                                                      .AllowAnyMethod()
                                                      .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("DefaultPolicy");

var scope = app.Services.CreateScope();
await DataUtility.ManageDataAsync(scope.ServiceProvider);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Set the Swagger endpoint
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PublicAPI v1");
    c.InjectStylesheet("/css/swagger.css"); // Inject custom CSS into Swagger UI
    c.InjectJavascript("/js/swagger.js"); // Inject custom JavaScript into Swagger UI
    c.DocumentTitle = "Dev Diary Documentation"; // Set the title of the Swagger UI document
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles();
app.UseRouting();


app.UseAuthorization();

// Custom BLogPost Details Route
app.MapControllerRoute(
    name: "custom",
    pattern: "Content/{slug}",
    defaults: new { controller = "BlogPosts", action = "Details" }
    );

// Default route configuration 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BlogPosts}/{action=Index}/{id?}");
app.MapRazorPages();

// Run the application
app.Run();
