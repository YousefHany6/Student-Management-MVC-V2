using System.ComponentModel.DataAnnotations;

namespace FStudentManagement.Models
{
 public class ChildStage
 {
  [Key]
  public int ChildStageId { get; set; }
  [Required(ErrorMessage ="ادخل اسم الجروب ")]
  public string Name { get; set; }
  [Required(ErrorMessage = "ادخل المراحل الدراسية")]
  public int ParentStageId { get; set; }
  public virtual ParentStage? ParentStage { get; set; }
		public virtual ICollection<TeacherStageCourse>? TeacherStageCourses { get; set; }
		public virtual ICollection<StudentTeacher>? StudentTeachers { get; set; }

		public virtual ICollection<Student>? Students { get; set; }
		public const string Collage_Name = "تكنولوجيا المعلومات";
		public const string College_department = "شبكات";
	}
}
