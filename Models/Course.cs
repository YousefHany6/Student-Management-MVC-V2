using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FStudentManagement.Models
{
	public class Course
	{
		[Key]
		public int Course_ID { get; set; }

		[Required(ErrorMessage = "ادخل اسم المادة")]
		public string Course_Name { get; set; }

		public const string Collage_Name = "تكنولوجيا المعلومات";
		public const string College_department = "شبكات";


		public virtual ICollection<StudentCourse>? StudentCourses { get; set; }

		// Navigation properties for stages
		public virtual ICollection<Attendance>? cAttendances { get; set; }
		[Required(ErrorMessage = "ادخل المراحل الدراسية")]

		public int ParentStageId { get; set; }
		public virtual ParentStage? ParentStage { get; set; }


		public virtual ICollection<TeacherStageCourse>? TeacherStageCourses { get; set; }

		public  ICollection<StudentTeacher>? StudentTeachers { get; set; }


	}
}
