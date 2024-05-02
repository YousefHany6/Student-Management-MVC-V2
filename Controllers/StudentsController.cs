using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FStudentManagement.Models;
using FStudentManagement.Rpo_models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NuGet.DependencyResolver;
using System.Linq.Expressions;
using FStudentManagement.Data;

namespace FStudentManagement.Controllers
{
	[Authorize(Roles = "Admin")]

	public class StudentsController : Controller
	{
		private readonly IRepository<StudentAttendance> studentatt;
		private readonly IRepository<TeacherStageCourse> teacherstage;
		private readonly IRepository<Student> std;
		private readonly UserManager<SUser> userManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly IRepository<StudentTeacher> student_Teacher;
		private readonly IRepository<StudentCourse> student_Courses;
		private readonly IRepository<Course> course;
		private readonly IRepository<ParentStage> parent;
		private readonly IRepository<ChildStage> child;

		public StudentsController(IRepository<StudentAttendance> Studentatt, IRepository<TeacherStageCourse> teacherstage, IRepository<Student> Student, UserManager<SUser> userManager, RoleManager<IdentityRole> roleManager, IRepository<StudentTeacher> Student_Teacher, IRepository<StudentCourse> Student_courses, IRepository<Course> Course, IRepository<ParentStage> parent, IRepository<ChildStage> child)
		{
			studentatt = Studentatt;
			this.teacherstage = teacherstage;
			std = Student;
			this.userManager = userManager;
			this.roleManager = roleManager;
			student_Teacher = Student_Teacher;
			student_Courses = Student_courses;
			course = Course;
			this.parent = parent;
			this.child = child;
		}
		public async Task<IActionResult> Add_Student()
		{
			ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
			ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add_Student(Student student,string CountryCode)
		{
			if (ModelState.IsValid && student != null)
			{
				var email = await userManager.FindByEmailAsync(student.SUser.Email);
				if (email != null)
				{
					ModelState.AddModelError(string.Empty, "هذا الايميل موجود يرجى تغيير الايميل الحالى");
					ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
					ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
					return View();
				}
				if (student.SUser.PasswordHash == null)
				{
					student.SUser.PasswordHash = student.SUser.Email;
				}
				var user = new SUser
				{
					FullName = student.Student_Name.TrimStart().TrimEnd(),
					Email = student.SUser.Email.TrimStart().TrimEnd(),
					UserName = student.SUser.Email.TrimStart().TrimEnd()
				};
				var result = await userManager.CreateAsync(user, student.SUser.PasswordHash);

				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, "Student");

					student.SUser = user;
					student.PhoneNumber = $"+{CountryCode}{student.PhoneNumber.Trim()}";
					

					await std.Create(student);
					var courses = await course.GetFilteredAsync(s => s.ParentStageId == student.ParentStageId);
					if (courses == null)
					{
						ModelState.AddModelError(string.Empty, "لا يوجد مواد يرجى اضافة مواد للمرحلة");
						ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
						ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
						await std.Delete(student);
						await userManager.DeleteAsync(user);
						return View();
					}
					foreach (var course in courses)
					{
						var stco = new StudentCourse
						{
							StudentID = student.Student_ID,
							CourseID = course.Course_ID
						};
						await student_Courses.Create(stco);
						var getth = await teacherstage.filterone(s => s.ParentStageId == student.ParentStageId && s.CourseId == course.Course_ID && s.childid == student.ChildStageId);
						if (getth == null)
						{
							ModelState.AddModelError(string.Empty, $"لا يوجد معلم للمادة {course.Course_Name}");
							ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
							ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
							await std.Delete(student);
							await userManager.DeleteAsync(user);
							return View();
						}
						var thstd = new StudentTeacher
						{
							Student_ID = student.Student_ID,
							CourseID = course.Course_ID,
							Teacher_ID = getth.TeacherId,
							childid = student.ChildStageId
						};
						await student_Teacher.Create(thstd);



					}

				}
				return RedirectToAction(nameof(Add_Student));


			}

			return View(student);
		}


		public async Task<IActionResult> display_Student()
		{
			ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
			ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]

		public async Task<IActionResult> display_Student(int parentid, int childid)
		{
			ViewBag.courses = new SelectList(await course.GetAll(), "Course_ID", "Course_Name");
			ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
			ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");

			if (parentid != 0 && childid != 0)
			{
				Expression<Func<Student, bool>> filter = t =>
											t.ParentStageId == parentid && t.ChildStageId == childid;
				var studentWithCourses = await std.GetFilteredAsync(
							filter,
							e => e.StudentCourses,ee=>ee.ChildStage
			);
				return View(studentWithCourses);

			}
			else if (parentid != 0 && childid == 0)
			{
				var studentWithCourses = await std.GetFilteredAsync(s => s.ParentStageId == parentid, e => e.StudentCourses
);
				return View(studentWithCourses);

			}
			else
			{
				ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
				ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");

				ModelState.AddModelError(string.Empty, "يرجى اختيار مرحلة");

				return View("display_Student");
			}
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> delete_Student(int id)
		{
			var stud = await std.GetById(id);

			if (stud != null && stud.userid != null)
			{
				await student_Courses.DeleteWhere(s => s.StudentID == id);
				await student_Teacher.DeleteWhere(s => s.Student_ID == id);
				await studentatt.DeleteWhere(s => s.StudentId == id);
				await std.Delete(stud);
				var user = await userManager.FindByIdAsync(stud.userid);

				if (user != null)
				{
					var result = await userManager.DeleteAsync(user);

					if (result.Succeeded)
					{

						return RedirectToAction("display_Student");
					}
				}
			}

			return RedirectToAction("display_Student");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit_Student(int id, int parentid, int childid)
		{
			ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name", parentid);

			ViewBag.Childs = new SelectList(await child.GetFilteredAsync(s=>s.ParentStageId==parentid), "ChildStageId", "Name", childid);
			var st = await std.GetById(id);
			ViewBag.user = await userManager.FindByIdAsync(st.userid);
			return View(st);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit_done(Student student,string CountryCode)
		{
			if (ModelState.IsValid && student != null)
			{
				var user = await userManager.FindByIdAsync(student.userid);

				if (user != null)
				{
					user.FullName = student.Student_Name.TrimStart().TrimEnd();
					user.SecurityStamp = student.SUser.SecurityStamp;
					user.ConcurrencyStamp = student.SUser.ConcurrencyStamp;

					if (student.SUser.PasswordHash != null)
					{
						user.PasswordHash = userManager.PasswordHasher.HashPassword(user, student.SUser.PasswordHash);
					}

					var updateResult = await userManager.UpdateAsync(user);

					if (updateResult.Succeeded)
					{
						var oldstd = await std.GetById(student.Student_ID);
						if (oldstd.ParentStageId != student.ParentStageId|| oldstd.ChildStageId != student.ChildStageId)
						{
							var courseold = await course.GetFilteredAsync(s => s.ParentStageId == oldstd.ParentStageId);
							if (courseold == null)
							{
								ModelState.AddModelError(string.Empty, "لا يوجد مواد يرجى اضافة مواد للمرحلة");
								ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
								ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
								return View("Edit_Student");
							}
							foreach (var item in courseold)
							{
								await student_Courses.DeleteWhere(s => s.StudentID == student.Student_ID && s.CourseID == item.Course_ID);
								var getth = await teacherstage.filterone(s => s.ParentStageId == oldstd.ParentStageId && s.CourseId == item.Course_ID && s.childid == oldstd.ChildStageId);
								await student_Teacher.DeleteWhere(s => s.Student_ID == student.Student_ID && s.CourseID == item.Course_ID && s.Teacher_ID == getth.TeacherId);



							}

							var courses = await course.GetFilteredAsync(s => s.ParentStageId == student.ParentStageId);
							if (courses == null)
							{
								ModelState.AddModelError(string.Empty, "لا يوجد مواد يرجى اضافة مواد للمرحلة");
								ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
								ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
								return View("Edit_Student");
							}
							foreach (var course in courses)
							{
								var stco = new StudentCourse
								{
									StudentID = student.Student_ID,
									CourseID = course.Course_ID
								};
								await student_Courses.Create(stco);
								var getth = await teacherstage.filterone(s => s.ParentStageId == student.ParentStageId && s.CourseId == course.Course_ID && s.childid == student.ChildStageId);
								if (getth == null)
								{
									ModelState.AddModelError(string.Empty, $"لا يوجد معلم للمادة {course.Course_Name}");
									ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
									ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
									var st = await std.GetById(student.Student_ID);
									ViewBag.user = await userManager.FindByIdAsync(st.userid);
									return View("Edit_Student", st);
								}
								var thstd = new StudentTeacher
								{
									Student_ID = student.Student_ID,
									CourseID = course.Course_ID,
									Teacher_ID = getth.TeacherId,
									childid = student.ChildStageId
								};
								await student_Teacher.Create(thstd);



							}
						}
						std.Detach(oldstd);
						student.SUser = user;
						student.PhoneNumber = $"+{CountryCode}{student.PhoneNumber.Trim()}";
						await std.Update(student);


						return RedirectToAction("display_Student");
					}
				}
			}

			return RedirectToAction("display_Student");
		}

	}
}
