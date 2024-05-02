using FStudentManagement.Data;
using FStudentManagement.Models;
using FStudentManagement.Rpo_models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NuGet.DependencyResolver;
using NuGet.Protocol.Core.Types;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace FStudentManagement.Controllers
{
	[Authorize(Roles = "Admin")]
	public class TeachersController : Controller
	{
		private readonly UserManager<SUser> userManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly IRepository<Attendance> att_Teacher;
		private readonly IRepository<Student> std;
		private readonly IRepository<StudentTeacher> std_Teacher;
		private readonly IRepository<TeacherStageCourse> teacherStage;
		private readonly IRepository<Teacher> Teacher;
		private readonly IRepository<ParentStage> parent;
		private readonly IRepository<ChildStage> child;
		private readonly IRepository<Course> Course;


		public TeachersController(UserManager<SUser> userManager, RoleManager<IdentityRole> roleManager, IRepository<Attendance> att_Teacher, IRepository<Student> std, IRepository<StudentTeacher> std_Teacher, IRepository<TeacherStageCourse> TeacherStage, IRepository<Teacher> Teacher, IRepository<Course> Course, IRepository<ParentStage> parent, IRepository<ChildStage> child)
		{
			this.userManager = userManager;
			this.roleManager = roleManager;
			this.att_Teacher = att_Teacher;
			this.std = std;
			this.std_Teacher = std_Teacher;
			teacherStage = TeacherStage;
			this.Teacher = Teacher;
			this.parent = parent;
			this.child = child;
			this.Course = Course;



		}
  public async Task<IActionResult> display_th()
  {
			return View(await Teacher.GetAllTeachersWithRelatedDataAsync(

				));
  }

  public async Task<IActionResult> Add_Teacher()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add_Teacher(Teacher teacher)
		{
			if (ModelState.IsValid && teacher != null)
			{
				var email = await userManager.FindByEmailAsync(teacher.SUser.Email);
				if (email != null)
				{
					ModelState.AddModelError(string.Empty, "هذا الايميل موجود يرجى تغيير الايميل الحالى");
					return View();
				}
				if (teacher.SUser.PasswordHash == null)
				{
					teacher.SUser.PasswordHash = teacher.SUser.Email;
				}
				var user = new SUser
				{
					FullName = teacher.Teacher_Name.TrimStart().TrimEnd(),
					Email = teacher.SUser.Email.TrimStart().TrimEnd(),
					UserName = teacher.SUser.Email.TrimStart().TrimEnd()
				};
				var result = await userManager.CreateAsync(user, teacher.SUser.PasswordHash);

				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, "Teacher");

					teacher.SUser = user;
					await Teacher.Create(teacher);

				}

				return RedirectToAction(nameof(Add_Teacher));


			}

			return View(teacher);
		}


		[HttpGet]
		public async Task<IActionResult> GetChildStages(int parentId)
		{
			Expression<Func<ChildStage, bool>> filter = c => c.ParentStageId == parentId;

			var childStages = await child.GetFilteredAsync(filter);
			ViewBag.Childs = new SelectList(childStages, "ChildStageId", "Name");
			return Json(ViewBag.Childs);
		}

		public async Task<IActionResult> Getcourses(int parentId)
		{
			Expression<Func<Course, bool>> filter = c => c.ParentStageId == parentId;

			var course = await Course.GetFilteredAsync(filter);
			ViewBag.courses = new SelectList(course, "Course_ID", "Course_Name");
			return Json(ViewBag.courses);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete_Teacher_System(int id)
		{
			var teacher = await Teacher.GetById(id);
			if (teacher != null && teacher.userid != null)
			{
				Expression<Func<TeacherStageCourse, bool>> condition = tsc =>
																			tsc.TeacherId == id;
				await teacherStage.DeleteWhere(condition);
				await std_Teacher.DeleteWhere(s => s.Teacher_ID == id);
				await Teacher.Delete(teacher);
				var user = await userManager.FindByIdAsync(teacher.userid);

				if (user != null)
				{
					var result = await userManager.DeleteAsync(user);

					if (result.Succeeded)
					{
						// Redirect to Teachers action with the same stage parameters
						return RedirectToAction("display_th");
					}
				}

			}
			ModelState.AddModelError(Empty.ToString(), "خطأ لم يتم الحذف بشكل صحيح");
			return View("display_th");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> addstage(int thid )
		{
			ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");
			ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
			ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
			var th = await Teacher.filterone(s=>s.Teacher_ID==thid,s=>s.Include(sss=>sss.SUser));
			return View("addnewstage", th);
		}

		public async Task<IActionResult> addnewstage()
		{
			ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");
			ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
			ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> addnewstage(string emailth, int parid, List<int> grouplist, int courseid)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var user = await userManager.FindByEmailAsync(emailth);
			if (user == null)
			{
				ModelState.AddModelError(string.Empty, "هذا الايميل غير موجود");
				ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");
				ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
				ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
				return View();
			}
			var teacher = await Teacher.filterone(s => s.userid == user.Id);
			var checkstage = await teacherStage.GetFilteredAsync(s => s.TeacherId == teacher.Teacher_ID && s.ParentStageId == parid);
			if (checkstage.Count != 0)
			{
				ModelState.AddModelError(string.Empty, "هذا المعلم موجود فى هذه المرحلة");
				ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");
				ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
				ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
				return View();
			}
			foreach (var chiid in grouplist)
			{
				var checkcourse = await teacherStage.GetFilteredAsync(c => c.CourseId == courseid && c.childid == chiid);
				if (checkcourse.Count != 0)
				{
					var cccs = await child.GetById(chiid);
					var ccc = await Course.GetById(courseid);

					ModelState.AddModelError(string.Empty, $"يوجد معلم للمادة {ccc.Course_Name} مع {cccs.Name}");
					ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");
					ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
					ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
					return View();
				}
				var newstageteacher = new TeacherStageCourse
				{
					ParentStageId = parid,
					TeacherId = teacher.Teacher_ID,
					childid = chiid,
					CourseId = courseid
				};
				await teacherStage.Create(newstageteacher);

				var stds = await std.GetFilteredAsync(s => s.ParentStageId == parid && s.ChildStageId == chiid);
				if (stds.Count != 0)
				{

					foreach (var item in stds)
					{
						var stdth = new StudentTeacher
						{
							Teacher_ID = teacher.Teacher_ID,
							CourseID = courseid,
							Student_ID = item.Student_ID,
							childid = chiid

						};
						await std_Teacher.Create(stdth);
					}
				}
			}


			return RedirectToAction("addnewstage");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> edit_th(int id, int parentid, int courid)
		{
			ViewBag.courses = new SelectList(await Course.GetFilteredAsync(s => s.ParentStageId == parentid), "Course_ID", "Course_Name", courid);
			ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name", parentid);
			var th = await Teacher.GetById(id);
			ViewBag.user = await userManager.FindByIdAsync(th.userid);

			var allGroups = await teacherStage.GetFilteredAsync(tc => tc.TeacherId == th.Teacher_ID, ss => ss.ChildStage);

			ViewBag.Childs = allGroups.Select(st => new SelectListItem
			{
				Value = st.childid.ToString(),
				Text = st.ChildStage.Name,
				Selected = allGroups.Any()
			}).ToList();

			return View(th);
		}



		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> edit_done(Teacher teacher, int courseid, List<int> grouplist, int parid)
		{
			if (ModelState.IsValid && teacher != null)
			{

				var user = await userManager.FindByIdAsync(teacher.userid);

				if (user != null)
				{
					// Update Teacher entity
					user.FullName = teacher.Teacher_Name.TrimStart().TrimEnd();
					user.SecurityStamp = teacher.SUser.SecurityStamp;
					user.ConcurrencyStamp = teacher.SUser.ConcurrencyStamp;

					if (teacher.SUser.PasswordHash != null)
					{
						user.PasswordHash = userManager.PasswordHasher.HashPassword(user, teacher.SUser.PasswordHash);
					}

					var updateResult = await userManager.UpdateAsync(user);

					if (updateResult.Succeeded)
					{
						teacher.SUser = user;
						await Teacher.Update(teacher);
						await teacherStage.DeleteWhere(s => s.ParentStageId == parid && s.TeacherId == teacher.Teacher_ID && s.CourseId == courseid);
						foreach (var chiid in grouplist)
						{

							var stage = new TeacherStageCourse
							{
								CourseId = courseid,
								childid = chiid,
								ParentStageId = parid,
								TeacherId = teacher.Teacher_ID
							};
							await teacherStage.Create(stage);

						}


						return RedirectToAction("display_th");
					}
				}
			}
			return BadRequest();
		}

	}
}

