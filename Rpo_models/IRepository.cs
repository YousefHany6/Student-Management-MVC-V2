using FStudentManagement.Models;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace FStudentManagement.Rpo_models
{
 public interface IRepository<T> where T : class
 {
  Task<List<Teacher>> GetAllTeachersWithRelatedDataAsync();

		Task<IEnumerable<T>> GetAll();
  Task<T> GetById(int id);
  Task Create(T entity);
  Task Update(T entity);
  Task Delete(T entity);
 void Detach(T entity);
  Task<T> GetByemail(string email);
  Task<IEnumerable<T>> GetByCondition(Expression<Func<T, bool>> condition);
		Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);
  Task<bool> GetByName(Expression<Func<T, bool>> predicate);
  Task<IEnumerable<T>> GetAllWithIncludes(params Expression<Func<T, object>>[] includes);
  Task<T> GetByIdWithIncludeAsync(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] includes);
  Task<T> filterone(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);

		Task<IEnumerable<T>> Include(params Expression<Func<T, object>>[] includes);
  Task DeleteWhere(Expression<Func<T, bool>> condition);
  Task<T> GetByIdWithRelatedEntitiesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
  Task<List<T>> GetFilteredAsync(Expression<Func<T, bool>> filter,
                                                   params Expression<Func<T, object>>[] includes);

  Task<int> GetStageIdAsync(Expression<Func<TeacherStageCourse, bool>> filter, Expression<Func<TeacherStageCourse, int>> select);

	}
}