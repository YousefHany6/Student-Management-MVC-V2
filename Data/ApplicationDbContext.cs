using FStudentManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FStudentManagement.Data
{
 public class ApplicationDbContext : IdentityDbContext<SUser>
 {
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                  : base(options)
  {
  }
  protected override void OnModelCreating(ModelBuilder builder)
  {
   base.OnModelCreating(builder);
   #region Student_Relationship
   builder.Entity<Student>()
       .HasOne(s => s.SUser)
                       .WithOne()
                       .HasForeignKey<Student>(s => s.userid)
       .OnDelete(DeleteBehavior.Restrict);



   builder.Entity<StudentCourse>()
       .HasKey(s => new { s.StudentID, s.CourseID });

   builder.Entity<StudentCourse>()
       .HasOne(p => p.Student)
       .WithMany(p => p.StudentCourses)
       .HasForeignKey(p => p.StudentID).OnDelete(DeleteBehavior.Cascade);


   builder.Entity<StudentCourse>()
.HasOne(p => p.Course)
.WithMany(p => p.StudentCourses)
.HasForeignKey(p => p.CourseID).OnDelete(DeleteBehavior.Cascade);



   builder.Entity<StudentTeacher>()
       .HasKey(p => new { p.Student_ID, p.Teacher_ID, p.CourseID, p.childid });

   builder.Entity<StudentTeacher>()
       .HasOne(s => s.Student)
       .WithMany(s => s.StudentTeachers)
       .HasForeignKey(s => s.Student_ID).OnDelete(DeleteBehavior.Cascade);

   builder.Entity<StudentTeacher>()
          .HasOne(s => s.ChildStage)
          .WithMany(s => s.StudentTeachers)
          .HasForeignKey(s => s.childid).OnDelete(DeleteBehavior.Cascade);


   builder.Entity<StudentTeacher>()
            .HasOne(s => s.Teacher)
            .WithMany(s => s.StudentTeachers)
            .HasForeignKey(s => s.Teacher_ID).OnDelete(DeleteBehavior.Cascade);

   builder.Entity<StudentTeacher>()
                           .HasOne(st => st.Course)
                           .WithMany(c => c.StudentTeachers)
                           .HasForeignKey(st => st.CourseID).OnDelete(DeleteBehavior.Cascade);


   builder.Entity<Student>()
                                                       .HasOne(s => s.ParentStage)
                                                       .WithMany(s => s.Students)
                                                       .HasForeignKey(s => s.ParentStageId)
       .OnDelete(DeleteBehavior.Restrict);

   builder.Entity<Student>()
                   .HasOne(s => s.ChildStage)
                   .WithMany(s => s.Students)
                   .HasForeignKey(s => s.ChildStageId)
       .OnDelete(DeleteBehavior.Restrict);


   #endregion

   #region Teacher_Relationship
   builder.Entity<Teacher>()
.HasOne(s => s.SUser)
           .WithOne()
           .HasForeignKey<Teacher>(s => s.userid)
       .OnDelete(DeleteBehavior.Restrict);

   #endregion

   #region Course_RalationShip
   builder.Entity<Course>()
                                                       .HasOne(c => c.ParentStage)
                                                       .WithMany(c => c.Courses)
                                                       .HasForeignKey(c => c.ParentStageId)
       .OnDelete(DeleteBehavior.Restrict);


   #endregion


   builder.Entity<TeacherStageCourse>()
                  .HasKey(tsc => new { tsc.TeacherId, tsc.ParentStageId, tsc.childid, tsc.CourseId });

   builder.Entity<TeacherStageCourse>()
                   .HasOne(tsc => tsc.Teacher)
                   .WithMany(r => r.TeacherStageCourses)
                   .HasForeignKey(tsc => tsc.TeacherId)
                   .IsRequired().OnDelete(DeleteBehavior.Cascade);


   builder.Entity<TeacherStageCourse>()
                   .HasOne(tsc => tsc.ChildStage)
                   .WithMany(r => r.TeacherStageCourses)
                   .HasForeignKey(tsc => tsc.childid)
                   .IsRequired().OnDelete(DeleteBehavior.Cascade);

   builder.Entity<TeacherStageCourse>()
                   .HasOne(tsc => tsc.Course)
                   .WithMany(r => r.TeacherStageCourses)
                   .HasForeignKey(tsc => tsc.CourseId)
                   .IsRequired().OnDelete(DeleteBehavior.Cascade);

   builder.Entity<TeacherStageCourse>()
                   .HasOne(tsc => tsc.ParentStage)
                   .WithMany(r => r.TeacherStageCourses)
                   .HasForeignKey(tsc => tsc.ParentStageId)
                   .IsRequired().OnDelete(DeleteBehavior.Restrict);
   ;

   builder.Entity<Attendance>()
                                                       .HasOne(a => a.Course)
                                                       .WithMany(c => c.cAttendances)
                                                       .HasForeignKey(a => a.Course_ID)
                   .IsRequired().OnDelete(DeleteBehavior.Cascade);





   builder.Entity<StudentAttendance>()
                                       .HasKey(sa => new { sa.StudentId, sa.AttendanceId });

   builder.Entity<StudentAttendance>()
                   .HasOne(sa => sa.Student)
                   .WithMany(s => s.StudentAttendances)
                   .HasForeignKey(sa => sa.StudentId).IsRequired().OnDelete(DeleteBehavior.Cascade);



   builder.Entity<StudentAttendance>()
                   .HasOne(sa => sa.Attendance)
                   .WithMany(a => a.StudentAttendances)
                   .HasForeignKey(sa => sa.AttendanceId).OnDelete(DeleteBehavior.Cascade);





  }
  public DbSet<ChildStage> childStages { get; set; }
  public DbSet<Course> Courses { get; set; }
  public DbSet<ParentStage> parentStages { get; set; }
  public DbSet<Student> Students { get; set; }
  public DbSet<StudentCourse> StudentCourses { get; set; }
  public DbSet<StudentTeacher> studentTeachers { get; set; }
  public DbSet<Teacher> Teachers { get; set; }
  public DbSet<TeacherStageCourse> teacherStageCourses { get; set; }
  public DbSet<Attendance> Attendances { get; set; }
  public DbSet<StudentAttendance> StudentAttendances { get; set; }


 }
}
