namespace FStudentManagement.Models
{
	public class StudentAttendance
	{
		public int StudentId { get; set; }
		public virtual Student Student { get; set; }

		public int AttendanceId { get; set; }
		public virtual Attendance Attendance { get; set; }

		public bool IsPresent { get; set; } 

	}
}
