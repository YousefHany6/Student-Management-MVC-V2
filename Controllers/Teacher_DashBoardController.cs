using FStudentManagement.Data;
using FStudentManagement.Models;
using FStudentManagement.Rpo_models;
using FStudentManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mono.TextTemplating;

namespace FStudentManagement.Controllers
{
	[Authorize(Roles = "Teacher")]

	public class Teacher_DashBoardController : Controller
	{
		private readonly IRepository<Attendance> att;
		private readonly IRepository<StudentAttendance> stdatt;
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


  public Teacher_DashBoardController(IRepository<Attendance> att, IRepository<StudentAttendance> stdatt,UserManager<SUser> userManager, RoleManager<IdentityRole> roleManager, IRepository<Attendance> att_Teacher, IRepository<Student> std, IRepository<StudentTeacher> std_Teacher, IRepository<TeacherStageCourse> TeacherStage, IRepository<Teacher> Teacher, IRepository<Course> Course, IRepository<ParentStage> parent, IRepository<ChildStage> child)
  {
			this.att = att;
			this.stdatt = stdatt;
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

		public async Task<IActionResult> choosegroup()  
		{
			var user = await userManager.GetUserAsync(User);
			var th = await Teacher.filterone(s=>s.userid==user.Id);
   ViewBag.Groups = new SelectList(await child.GetFilteredAsync(s=>s.TeacherStageCourses.Any(s=>s.TeacherId==th.Teacher_ID)),"ChildStageId","Name");
			ViewBag.courses = new SelectList(await Course.GetFilteredAsync(s=>s.TeacherStageCourses.Any(s=>s.TeacherId==th.Teacher_ID)), "Course_ID", "Course_Name");

			return View();
		}


		[HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Index(DateTime datenow, int parrid,int courseid)
		{
			var user = await userManager.GetUserAsync(User);
			var th = await Teacher.filterone(s => s.userid == user.Id); 

   var stds = await std.GetFilteredAsync(s=>s.StudentTeachers.Any(s=>s.Teacher_ID==th.Teacher_ID&&s.childid==parrid));


   ViewBag.Groups = new SelectList(await child.GetFilteredAsync(s => s.TeacherStageCourses.Any(s => s.TeacherId == th.Teacher_ID)), "ChildStageId", "Name");

   ViewBag.co = courseid;

   ViewBag.date=datenow;

			return View(stds);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> save_table(List<int> attendanceStates, List<int> stds, int courseid1, DateTime datenow1)
		{
			var ta = new Attendance()
			{
				Date = datenow1,
				Course_ID = courseid1	
			};

			await att.Create(ta);

			foreach (var st in stds)
			{
				bool isPresent = attendanceStates.Contains(st);

				var st_std = new StudentAttendance
				{
					AttendanceId = ta.AttendanceId,
					StudentId = st,
					IsPresent = isPresent
				};

				await stdatt.Create(st_std);

				if (isPresent == false)
				{
					var student = await std.GetById(st);
					var course = await Course.GetById(courseid1);
					await send_message_what_sup.Send_Message(student.PhoneNumber, student.Student_Name, datenow1, course.Course_Name);

				}
			}
			return RedirectToAction("choosegroup", "Teacher_DashBoard");
		}

	}
}
