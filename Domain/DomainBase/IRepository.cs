using Domain.DomainBase;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DomainBase
{
    public interface IRepository<T> where T : Entity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        T Add(T t);
        /// <summary>
        /// 更新对象
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        void Update(T t);
        /// <summary>
        /// 条件更新对象
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="condition"></param>
        /// <param name="setSetProperty"></param>
        /// <param name="value"></param>
        void Update<TProperty>(Expression<Func<T, bool>> condition, Expression<Func<T, TProperty>> setSetProperty, TProperty value);
        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        void Delete(T t);
        /// <summary>
        /// 根据条件删除对象
        /// </summary>
        /// <param name="condition"></param>
        void Delete(Expression<Func<T, bool>> condition);
        Task DeleteAsync(Expression<Func<T, bool>> condition);
        /// <summary>
        /// 根据主键获取对象
        /// </summary>
        /// <returns></returns>
        Task<T> GetAsync(int key = 0, bool isDeleted = false);
        Task<T> GetAsync(Expression<Func<T, bool>> condition, bool isDeleted = false, List<SortedParams> sorteds = null);
        /// <summary>
        /// 判断对象是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isDeleted"></param>
        /// <returns></returns>
        Task<bool> AnyAsync(int key = 0, bool isDeleted = false);
        /// <summary>
        /// 根据条件判断对象是否存在
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="isDeleted"></param>
        /// <returns></returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> condition, bool isDeleted = false);
        /// <summary>
        /// 根据主键获取对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isDeleted"></param>
        /// <returns></returns>
        Task<List<T>> GetManyToListAsync(int[] key, bool isDeleted = false);
        /// <summary>
        /// 根据条件获取对象
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="sorteds"></param>
        /// <param name="isDeleted"></param>
        /// <returns></returns>
        Task<List<T>> GetManyAsync(Expression<Func<T, bool>> condition, List<SortedParams> sorteds = null, bool isDeleted = false);
        /// <summary>
        /// 根据条件获取分页对象
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sorteds"></param>
        /// <param name="isDeleted"></param>
        /// <returns></returns>
        Task<(int total, List<T> lists)> GetManyByPageAsync(Expression<Func<T, bool>> condition, int skip, int take, List<SortedParams> sorteds = null, bool isDeleted = false);
    }
}
