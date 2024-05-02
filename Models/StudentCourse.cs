namespace FStudentManagement.Models
{
 public class StudentCourse
 {
  public int StudentID { get; set; }
  public virtual Student Student { get; set; }

  public int CourseID { get; set; }
  public virtual Course Course { get; set; }
 }
}
