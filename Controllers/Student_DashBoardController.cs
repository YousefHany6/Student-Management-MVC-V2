using FStudentManagement.Data;
using FStudentManagement.Models;
using FStudentManagement.Rpo_models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FStudentManagement.Controllers
{
	[Authorize(Roles = "Student")]

	public class Student_DashBoardController : Controller
	{
		private readonly UserManager<SUser> userManager;
		private readonly IRepository<StudentAttendance> studentatt;
		private readonly IRepository<Student> std;
		private readonly IRepository<StudentCourse> student_Courses;

		public Student_DashBoardController(UserManager<SUser> userManager, IRepository<StudentAttendance> Studentatt, IRepository<Student> Student, IRepository<StudentCourse> Student_courses)
		{
			this.userManager = userManager;
			studentatt = Studentatt;
			std = Student;
			student_Courses = Student_courses;

		}
  public async Task<IActionResult> Index()
  {
			var user = await userManager.GetUserAsync(User);
			var student = await std.filterone(s => s.userid == user.Id);
			if (student != null)
   {
    var studentCourses = await student_Courses.GetFilteredAsync(sc => sc.StudentID == student.Student_ID, s => s.Course);

    var coursesWithAbsences = new List<CourseWithAbsenceViewModel>();

    foreach (var studentCourse in studentCourses)
    {
     var absences = await studentatt.GetFilteredAsync(sa => sa.Attendance.Course_ID == studentCourse.CourseID && sa.StudentId == student.Student_ID && !sa.IsPresent, s => s.Attendance.Course);

     var courseWithAbsence = new CourseWithAbsenceViewModel
     {
      Course = studentCourse.Course,
      AbsenceCount = absences.Count()
     };

     coursesWithAbsences.Add(courseWithAbsence);
    }

    return View(coursesWithAbsences);
   }

   return NotFound();
  }
 }
}
