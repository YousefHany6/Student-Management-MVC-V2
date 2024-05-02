namespace FStudentManagement.Models
{
 public class StudentTeacher
 {
  public int Teacher_ID { get; set; }
  public virtual Teacher Teacher { get; set; }

  public int Student_ID { get; set; }
  public virtual Student Student { get; set; }

		public int CourseID { get; set; }
		public virtual Course Course { get; set; }

		public int childid { get; set; }
		public virtual ChildStage ChildStage { get; set; }
	}
}
