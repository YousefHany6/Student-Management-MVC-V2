using System.Diagnostics.CodeAnalysis;

namespace FStudentManagement.Models
{
	public class Attendance
	{
		public int AttendanceId { get; set; }
		public DateTime Date { get; set; }
		// Other attendance properties
		public int Course_ID { get; set; }
		public virtual Course? Course { get; set; }
		public virtual ICollection<StudentAttendance>? StudentAttendances { get; set; }

		public const string Collage_Name = "تكنولوجيا المعلومات";
		public const string College_department = "شبكات";

	}
}
