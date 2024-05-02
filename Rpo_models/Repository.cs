using FStudentManagement.Data;
using FStudentManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FStudentManagement.Rpo_models
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _context;

		public Repository(ApplicationDbContext context)
		{
			_context = context;
		}
		public void Detach(T entity)
		{
			_context.Entry(entity).State = EntityState.Detached;
		}
		public async Task<IEnumerable<T>> GetAll()
		{
			return await _context.Set<T>().ToListAsync();
		}

		public async Task<T> GetById(int id)
		{
			return await _context.Set<T>().FindAsync(id);
		}
		public async Task<T> GetByemail(string email)
		{
			return await _context.Set<T>().FindAsync(email);
		}
		public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter)
		{
			return await _context.Set<T>().FirstOrDefaultAsync(filter);
		}
		public async Task<T> filterone(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
		{
			IQueryable<T> query = _context.Set<T>();

			if (include != null)
			{
				query = include(query);
			}

			return await query.FirstOrDefaultAsync(filter);
		}

		public async Task Create(T entity)
		{
			_context.Set<T>().Add(entity);
			await _context.SaveChangesAsync();
		}

		public async Task Update(T entity)
		{
			_context.Entry(entity).State = EntityState.Modified;
			await _context.SaveChangesAsync();
		}

		public async Task Delete(T entity)
		{
			_context.Set<T>().Remove(entity);
			await _context.SaveChangesAsync();
		}
		public async Task<IEnumerable<T>> GetAllWithIncludes(params Expression<Func<T, object>>[] includes)
		{
			IQueryable<T> query = _context.Set<T>();

			foreach (var include in includes)
			{
				query = query.Include(include);
			}

			return await query.ToListAsync();
		}
		public async Task<bool> GetByName(Expression<Func<T, bool>> predicate)
		{
			return await _context.Set<T>().AnyAsync(predicate);
		}

		public async Task<T> GetByIdWithIncludeAsync(Expression<Func<T, bool>> condition, params Expression<Func<T, object>>[] includes)
		{
			IQueryable<T> query = _context.Set<T>();

			foreach (var include in includes)
			{
				query = query.Include(include);
			}

			return await query.FirstOrDefaultAsync(condition);
		}
		public async Task<IEnumerable<T>> Include(params Expression<Func<T, object>>[] includes)
		{
			IQueryable<T> query = _context.Set<T>();

			query = includes.Aggregate(query, (current, include) => current.Include(include));

			return await query.ToListAsync();
		}
		public async Task DeleteWhere(Expression<Func<T, bool>> condition)
		{
			var entitiesToDelete = await _context.Set<T>().Where(condition).ToListAsync();
			_context.Set<T>().RemoveRange(entitiesToDelete);
			await _context.SaveChangesAsync();
		}
		public async Task<T> GetByIdWithRelatedEntitiesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
		{
			IQueryable<T> query = _context.Set<T>();

			foreach (var include in includes)
			{
				query = query.Include(include);
			}

			return await query.FirstOrDefaultAsync(predicate);
		}

		public async Task<List<T>> GetFilteredAsync(Expression<Func<T, bool>> filter,
																																																			params Expression<Func<T, object>>[] includes)
		{
			IQueryable<T> query = _context.Set<T>();

			foreach (var include in includes)
			{
				query = query.Include(include);
			}

			if (filter != null)
			{
				query = query.Where(filter);
			}

			return await query.ToListAsync();
		}
		public async Task<IEnumerable<T>> GetByCondition(Expression<Func<T, bool>> condition)
		{
			return await _context.Set<T>().Where(condition).ToListAsync();
		}
		public async Task<int> GetStageIdAsync(Expression<Func<TeacherStageCourse, bool>> filter, Expression<Func<TeacherStageCourse, int>> select)
		{
			var stage = await _context.teacherStageCourses
							.Where(filter)
							.Select(select)
							.FirstOrDefaultAsync();

			return stage;
		}

		public async Task<List<Teacher>> GetAllTeachersWithRelatedDataAsync()
		{
			return await _context.Teachers
							.Include(t => t.TeacherStageCourses)
											.ThenInclude(tc => tc.ParentStage)
							.Include(t => t.TeacherStageCourses)
											.ThenInclude(tc => tc.ChildStage)
												.Include(t => t.TeacherStageCourses)
											.ThenInclude(tc => tc.Course)
											.Include(tc=>tc.SUser)
							.ToListAsync();
		}
	}

}
