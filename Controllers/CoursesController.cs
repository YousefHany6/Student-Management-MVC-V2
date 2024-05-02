using FStudentManagement.Models;
using FStudentManagement.Rpo_models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Protocol.Core.Types;
using System.Linq.Expressions;

namespace FStudentManagement.Controllers
{
	[Authorize(Roles = "Admin")]

	public class CoursesController : Controller
	{
		private readonly IRepository<TeacherStageCourse> teacherStage;
		private readonly IRepository<Student> std;
		private readonly IRepository<StudentCourse> stdcou;
		private readonly IRepository<Attendance> att_Course;
		private readonly IRepository<Course> Course;
		private readonly IRepository<ParentStage> parent;
		private readonly IRepository<ChildStage> child;

		public CoursesController(IRepository<TeacherStageCourse> TeacherStage, IRepository<Student> std, IRepository<StudentCourse> stdcou, IRepository<Attendance> att_course, IRepository<Course> Course, IRepository<ParentStage> parent, IRepository<ChildStage> child)
		{
			teacherStage = TeacherStage;
			this.std = std;
			this.stdcou = stdcou;
			att_Course = att_course;
			this.Course = Course;
			this.parent = parent;
			this.child = child;
		}
		public async Task<IActionResult> Courses()
		{
			ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
			ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
			return View(await Course.GetAll());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create_Course(Course course)
		{
			if (ModelState.IsValid)
			{
				var check=await Course.GetByCondition(s=>s.Course_Name==course.Course_Name.TrimStart().TrimEnd() && s.ParentStageId==course.ParentStageId);
				if (check.Any())
				{
					ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
					ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");
					ModelState.AddModelError(string.Empty,"هذه المادة موجودة");
					return View(nameof(Courses), await Course.GetAll());
				}
				course.Course_Name = course.Course_Name.TrimStart().TrimEnd();
				await Course.Create(course);

				var stds=await std.GetFilteredAsync(s=>s.ParentStageId== course.ParentStageId);
				foreach (var stud in stds)
				{
					var s = new StudentCourse
					{
						StudentID=stud.Student_ID,
						CourseID=course.Course_ID
					};
					await stdcou.Create(s);
				}



				return RedirectToAction("Courses");
			}
   return BadRequest();

  }

		public async Task<IActionResult> Delete_Course(int id)
		{
			var getid=await Course.GetById(id);
			if (getid != null)
			{
	
				await att_Course.DeleteWhere(e => e.Course_ID == id);
				await stdcou.DeleteWhere(e => e.CourseID == id);
				await Course.Delete(getid);
				return RedirectToAction("Courses");

			}

			return RedirectToAction("Courses");

		}
		public async Task<IActionResult> Edit_Course(int id)
		{
			var getid = await Course.GetById(id);
			if (getid != null)
			{
				ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name",getid.ParentStage);
				ViewBag.cours = getid.ParentStageId;

				return View("Edit_Course", getid);

			}

			return RedirectToAction("Courses");

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit_Course(Course course, int coursee)
		{ 
			if (course != null)
			{
				course.ParentStageId = coursee;
    await Course.Update(course);
				return RedirectToAction("Courses");
			}

			return RedirectToAction("Courses");

		}


	}
}
