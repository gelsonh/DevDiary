using DevDiary.Data;
using DevDiary.Models;
using DevDiary.Services.Interfaces;
using DevDiary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration) ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");



builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.MigrationsHistoryTable(tableName: "BlogMigrationHistory", schema: "blog")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


// Edit this for custom role modifications
builder.Services.AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();


// Register Custom Services
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IEmailSender, EmailService>();

// Bind the email settings to the EmailSettings object
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSetting"));

builder.Services.AddMvc(); // more freedom to customize and edit

// Add API configs
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
    string xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});

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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PublicAPI v1");
    c.InjectStylesheet("/css/swagger.css");
    c.InjectJavascript("/js/swagger.js");

    c.DocumentTitle = "Dev Diary Documentation";
});

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();


app.UseAuthorization();

// Custom BLogPost Details Route
app.MapControllerRoute(
    name: "custom",
    pattern: "Content/{slug}",
    defaults: new { controller = "BlogPosts", action = "Details" }
    );


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BlogPosts}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
