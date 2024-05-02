using FStudentManagement.Data;
using Microsoft.AspNetCore.Identity;

namespace FStudentManagement.Services
{
	public class seed_roles
	{
		public static async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SUser>>();

				await CreateRolesAndAdminUser(roleManager, userManager);
			}
		}

		private static async Task CreateRolesAndAdminUser(RoleManager<IdentityRole> roleManager, UserManager<SUser> userManager)
		{
			string[] roleName =["Admin", "Teacher", "Student"];
			string userName = "admin@admin.com";
			string password = "Admin1!@";
			foreach (var item in roleName)
			{
				if (!await roleManager.RoleExistsAsync(item))
				{
					await roleManager.CreateAsync(new IdentityRole(item));
				}
			}



			var user = await userManager.FindByNameAsync(userName);
			if (user == null)
			{
				user = new SUser { FullName = roleName[0], UserName = userName, Email = userName ,EmailConfirmed=true};
				await userManager.CreateAsync(user, password);
			}

			if (!await userManager.IsInRoleAsync(user, roleName[0]))
			{
				await userManager.AddToRoleAsync(user, roleName[0]);
			}
		}
	}
}
