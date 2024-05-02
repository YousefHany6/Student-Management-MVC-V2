using FStudentManagement.Data;
using FStudentManagement.Models;
using FStudentManagement.Rpo_models;
using FStudentManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FStudentManagement
{
    public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
							options.UseSqlServer(connectionString));

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();
			builder.Services.Configure<RequestLocalizationOptions>(options =>
			{
				options.DefaultRequestCulture = new RequestCulture("ar"); // Set default culture to Arabic
				options.SupportedCultures = new List<CultureInfo> { new CultureInfo("ar") }; // Supported cultures
				options.SupportedUICultures = new List<CultureInfo> { new CultureInfo("ar") }; // Supported UI cultures
			});
			builder.Services.AddIdentity<SUser, IdentityRole>()
							.AddEntityFrameworkStores<ApplicationDbContext>()
							.AddDefaultUI()
							.AddDefaultTokenProviders();
			builder.Services.AddControllersWithViews();
			builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


			builder.Services.Configure<IdentityOptions>(options =>
				{
					options.Password.RequiredLength = 8; 
					options.Password.RequireDigit = false; 
					options.Password.RequireLowercase = false; 
					options.Password.RequireUppercase = false; 
					options.Password.RequireNonAlphanumeric = false; 

					
					options.Password.RequiredUniqueChars = 0; 
					options.Password.RequiredUniqueChars = 0; 
					options.Password.RequiredUniqueChars = 0; 
				});
			

			var app = builder.Build();

					if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllerRoute(
							name: "default",
							pattern: "{controller=Manage}/{action=Index}/{id?}");

			app.MapRazorPages();

			seed_roles.InitializeAsync(app.Services).Wait();

			app.Run();
		}
	}
}
