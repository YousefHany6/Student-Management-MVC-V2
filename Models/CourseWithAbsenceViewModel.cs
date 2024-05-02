namespace FStudentManagement.Models
{
	public class CourseWithAbsenceViewModel
	{
		public virtual Course Course { get; set; }
		public int AbsenceCount { get; set; }
	}
}
