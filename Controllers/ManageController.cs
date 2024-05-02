using FStudentManagement.Data;
using FStudentManagement.Models;
using FStudentManagement.Rpo_models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FStudentManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IO;
using System.Linq.Expressions;

namespace FStudentManagement.Controllers
{
    [Authorize(Roles = "Admin")]

	public class ManageController : Controller
 {
  private readonly IRepository<Attendance> att;
  private readonly IRepository<StudentAttendance> stdatt;
  private readonly IRepository<Student> std;
		private readonly UserManager<SUser> userManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly IRepository<TeacherStageCourse> teacherStage;
		private readonly IRepository<Teacher> Teacher;
		private readonly IRepository<ParentStage> parent;
		private readonly IRepository<ChildStage> child;
		private readonly IRepository<Course> Course;


		public ManageController(IRepository<Attendance>  att, IRepository<StudentAttendance> stdatt,IRepository<Student> Student,UserManager<SUser> userManager, RoleManager<IdentityRole> roleManager, IRepository<TeacherStageCourse> TeacherStage, IRepository<Teacher> Teacher, IRepository<Course> Course, IRepository<ParentStage> parent, IRepository<ChildStage> child)
		{
   this.att = att;
   this.stdatt = stdatt;
   std = Student;
			this.userManager = userManager;
			this.roleManager = roleManager;
			teacherStage = TeacherStage;
			this.Teacher = Teacher;
			this.parent = parent;
			this.child = child;
			this.Course = Course;



		}


		public async Task <IActionResult> Index()
  {
   var user = await userManager.GetUserAsync(User);
   if (User.IsInRole("Teacher"))
   {
				return RedirectToAction("choosegroup","Teacher_DashBoard");

			}
			else if (User.IsInRole("Student"))
   {
				return RedirectToAction("Index", "Student_DashBoard");

			}
			return View();
  }
  public async Task<IActionResult> Create_Table()
  {
			ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");
			ViewBag.parents = new SelectList(await parent.GetAll(), "ParentStageId", "Name");
			ViewBag.Childs = new SelectList(await child.GetAll(), "ChildStageId", "Name");

			return View();
  }
		[HttpPost]
		[ValidateAntiForgeryToken]

  public async Task<IActionResult> create_table(DateTime datenow, int courseid, int childid, int parentid)
		{
   var students = await std.GetFilteredAsync(s=>s.ChildStageId==childid&& s.ParentStageId==parentid&&s.StudentCourses.Any(ss=>ss.CourseID==courseid));
   ViewBag.date = datenow;
   ViewBag.co = courseid;

   return View("attendance", students);
		}
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> attendance(List<int> attendanceStates, List<int> stds, int courseid1, DateTime datenow1)
  {
   var ta = new Attendance()
   {
    Date = datenow1,
    Course_ID = courseid1   };

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

    if (isPresent==false)
    {
     var student = await std.GetById(st);
     var course = await Course.GetById(courseid1);
				await	send_message_what_sup.Send_Message(student.PhoneNumber,student.Student_Name,datenow1,course.Course_Name);

				}
   }

   return RedirectToAction("Create_Table");
  }

  public async Task<IActionResult> Edit_attendance()
  {
   ViewBag.att = new SelectList(await att.GetAll(),"AttendanceId", "Date");
   ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");
   ViewBag.teachers = new SelectList(await Teacher.GetAll(), "Teacher_ID", "Teacher_Name");


   return View();
  }
  public async Task<IActionResult> GetDatesByTeacherAndCourse( int Course_ID)
  {
   try
   {
    var dates = await att.GetFilteredAsync(
               filter:a=> a.Course_ID == Course_ID

											);
    ViewBag.att = new SelectList(dates, "AttendanceId", "Date");

    return Json(ViewBag.att);
   }
   catch (Exception ex)
   {
    return BadRequest(ex.Message);
   }
  }
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit_attendance(int tableid)
  {
   try
   {

    if (tableid != 0)
    {
     ViewBag.table = tableid;
     var studentAttendances = await stdatt.GetFilteredAsync(sa => sa.AttendanceId == tableid, sa => sa.Student);

     return View("display_att", studentAttendances);
    }
    else
    {
					ModelState.AddModelError(string.Empty, "يرجى اختيار جدول صحيح");
					ViewBag.att = new SelectList(await att.GetAll(), "AttendanceId", "Date");
					ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");

					return View("Edit_attendance");
				}
   }
   catch (Exception ex)
   {
    return BadRequest(ex.Message);
   }
  }
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> edit_done(List<int> students,List<int> attstate,int tabid)
  {

   foreach (var st in students)
   {
    bool isPresent = attstate.Contains(st);

    var st_std = new StudentAttendance
    {
     AttendanceId = tabid,
     StudentId = st,
     IsPresent = isPresent
    };
    var stdold = await stdatt.filterone(s=>s.StudentId==st&&s.AttendanceId==tabid,ss=>ss.Include(sss=>sss.Student));
				stdatt.Detach(stdold);
				if ((stdold.IsPresent==true)&&(isPresent==false))
    {
					var attt = await att.filterone(sd=>sd.AttendanceId==tabid,sc=>sc.Include(c=>c.Course));
					att.Detach(attt);

					await send_message_what_sup.Send_Message(stdold.Student.PhoneNumber, stdold.Student.Student_Name, attt.Date,attt.Course.Course_Name);

				}
				

				await stdatt.Update(st_std);
   }

   return RedirectToAction("Edit_attendance");
  
 }
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> delete_Table(int id)
  {
   if (id != 0)
   {
   var ta=await att.GetById(id);
    await stdatt.DeleteWhere(s=>s.AttendanceId==id);
    await att.Delete(ta);
    return RedirectToAction("Edit_attendance");

   }
   else
   {
    ModelState.AddModelError(string.Empty, "يرجى اختيار جدول صحيح");
    ViewBag.att = new SelectList(await att.GetAll(), "AttendanceId", "Date");
    ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");
    ViewBag.teachers = new SelectList(await Teacher.GetAll(), "Teacher_ID", "Teacher_Name");

    return View("Edit_attendance");

   }

  }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> false_std(int tableid)
		{
			try
			{

				if (tableid != 0)
				{
     var getcourse=await att.GetById(tableid);
     var co = await Course.GetById(getcourse.Course_ID);
     ViewBag.course = co.Course_Name;
     ViewBag.date = getcourse.Date;
					var studentAttendances = await stdatt.GetFilteredAsync(sa => sa.AttendanceId == tableid && sa.IsPresent==false, sa => sa.Student);

					return View("false_std", studentAttendances);
				}
				else
				{
					ModelState.AddModelError(string.Empty, "يرجى اختيار جدول صحيح");
					ViewBag.att = new SelectList(await att.GetAll(), "AttendanceId", "Date");
					ViewBag.courses = new SelectList(await Course.GetAll(), "Course_ID", "Course_Name");

					return View("Edit_attendance");
				}
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

  public async Task<IActionResult> send_massage(string phone_number,string name,DateTime date,string course)
  {
   string message = $"الطالب {name} قد تغيب يوم {date} عن مادة {course}";
			string whatsappUrl = $"https://wa.me/{phone_number}?text={Uri.EscapeDataString(message)}";

			return Redirect(whatsappUrl);
		}
	}
}
