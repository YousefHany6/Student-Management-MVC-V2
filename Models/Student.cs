using FStudentManagement.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FStudentManagement.Models
{
    public class Student
 {
  [Key]
  public int Student_ID { get; set; }

		[Required(ErrorMessage = "ادخل اسم الطالب")]
		public string Student_Name { get; set; }
		[DataType(DataType.PhoneNumber)]
		[Required(ErrorMessage = "ادخل رقم الهاتف")]
		public string PhoneNumber { get; set; }
		public string? userid {  get; set; }
  public virtual SUser SUser { get; set; }

  public virtual ICollection<StudentCourse>? StudentCourses { get; set; }
  public virtual ICollection<StudentTeacher>? StudentTeachers { get; set; }
		public virtual ICollection<StudentAttendance>? StudentAttendances { get; set; }


		[Required(ErrorMessage = "اختر المرحلة")]
		public int ParentStageId { get; set; }
  public virtual ParentStage? ParentStage { get; set; }
		[Required(ErrorMessage = "اختر الجروب")]

		public int ChildStageId { get; set; }
  public virtual ChildStage? ChildStage { get; set; }

		public const string Collage_Name = "تكنولوجيا المعلومات";
		public const string College_department = "شبكات";

		public string CountryCode { get; set; }

	}
}
