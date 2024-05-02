using System.ComponentModel.DataAnnotations;

namespace FStudentManagement.Models
{
	public class TeacherStageCourse
	{
		public int TeacherId { get; set; }
		public virtual Teacher Teacher { get; set; }

		public int ParentStageId { get; set; }
		public virtual ParentStage ParentStage { get; set; }
		public int childid { get; set; }
		public virtual ChildStage ChildStage { get; set; }
		public int CourseId { get; set; }
		public virtual Course Course { get; set; }

	}
}
