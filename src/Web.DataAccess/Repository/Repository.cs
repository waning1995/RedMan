﻿using Microsoft.EntityFrameworkCore;
using RedMan.DataAccess.IRepository;
using RedMan.Model.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using RedMan.Model.Paging;

namespace RedMan.DataAccess.Repository
{
    public class Repository<T>:IRepository<T> where T:class
    {
        private readonly MyContext context;
        private readonly DbSet<T> dbSet;

        public Repository(MyContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }

        #region 单模型操作

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public bool Add(T entity,bool IsCommit = true)
        {
            dbSet.Add(entity);
            if (IsCommit)
                return context.SaveChanges() > 0;
            else
                return false;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public async Task<bool> AddAsync(T entity, bool IsCommit = true)
        {
            await dbSet.AddAsync(entity);
            if (IsCommit)
                return await context.SaveChangesAsync() > 0;
            else
                return await Task.Run(() => false);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public bool Update(T entity, bool IsCommit = true)
        {
            dbSet.Attach(entity);
            context.Entry<T>(entity).State = EntityState.Modified;
            if (IsCommit)
                return context.SaveChanges() > 0;
            else
                return false;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(T entity, bool IsCommit = true)
        {
            dbSet.Attach(entity);
            context.Entry<T>(entity).State = EntityState.Modified;
            if (IsCommit)
                return await context.SaveChangesAsync() > 0;
            else
                return false;
        }

        /// <summary>
        /// 添加或更新一条记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="IsCommit">是否提交</param>
        public bool AddOrUpdate(T entity, bool IsCommit = true)
        {
            if (Find(p => p.Equals(entity)) != null)
            {
                return Update(entity, IsCommit);
            }
            else
            {
                return Add(entity, IsCommit);
            }
        }

        /// <summary>
        /// 添加或更新一条记录
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="IsCommit">是否提交</param>
        public async Task<bool> AddOrUpdateAsync(T entity, bool IsCommit = true)
        {
            if (Find(p => p.Equals(entity)) != null)
            {
                return await UpdateAsync(entity, IsCommit);
            }
            else
            {
                return await AddAsync(entity, IsCommit);
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <returns></returns>
        public T Find(Expression<Func<T, bool>> predicate)
        {
            return dbSet.SingleOrDefault(predicate);
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <returns></returns>
        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await Task.Run(() => dbSet.SingleOrDefault(predicate));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="IsCommit">是否提交</param>
        public bool Delete(T entity, bool IsCommit = true)
        {
            if (entity == null)
                return false;
            //dbSet.Attach(entity);
            //context.Entry<T>(entity).State = EntityState.Deleted;

            dbSet.Remove(entity);
            if (IsCommit)
                return context.SaveChanges() > 0;
            else
                return false;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="IsCommit">是否提交</param>
        public async Task<bool> DeleteAsync(T entity, bool IsCommit = true)
        {
            if (entity == null)
                return await Task.Run(() => false);
            //dbSet.Attach(entity);
            //context.Entry<T>(entity).State = EntityState.Deleted;

            dbSet.Remove(entity);
            if (IsCommit)
                return await context.SaveChangesAsync() > 0;
            else
                return await Task.Run(() => false);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public bool Delete(Expression<Func<T, bool>> predicate, bool IsCommit = true)
        {
            IQueryable<T> entry = (predicate == null) ? dbSet.AsQueryable() : dbSet.Where(predicate);
            dbSet.RemoveRange(entry);
            if (IsCommit)
                return context.SaveChanges() > 0;
            else
                return false;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(Expression<Func<T, bool>> predicate, bool IsCommit = true)
        {
            IQueryable<T> entry = (predicate == null) ? dbSet.AsQueryable() : dbSet.Where(predicate);
            dbSet.RemoveRange(entry);
            if (IsCommit)
                return await context.SaveChangesAsync() > 0;
            else
                return await Task.Run(() => false);
        }

        #endregion

        #region 集合操作
        /// <summary>
        /// 添加列表
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public bool AddRange(IEnumerable<T> entities, bool IsCommit = true)
        {
            dbSet.AddRange(entities);
            if (IsCommit)
                return context.SaveChanges() > 0;
            else
                return false;
        }
        /// <summary>
        /// 添加列表
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="IsCommit">是否提交</param>
        public async Task<bool> AddRangeAsync(IEnumerable<T> entities, bool IsCommit = true)
        {
            dbSet.AddRange(entities);
            if (IsCommit)
                return await context.SaveChangesAsync() > 0;
            else
                return await Task.Run(() => false);
        }
        /// <summary>
        /// 更新列表
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public bool UpdateRange(IEnumerable<T> entities, bool IsCommit = true)
        {
            if (entities == null || entities.Count() == 0)
                return false;
            entities.AsParallel().ForAll(p => { dbSet.Attach(p); context.Entry(p).State = EntityState.Modified; });
            if (IsCommit)
                return context.SaveChanges() > 0;
            else
                return false;
        }
        /// <summary>
        /// 更新列表
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="IsCommit">是否提交</param>
        public async Task<bool> UpdateRangeAsync(IEnumerable<T> entities, bool IsCommit = true)
        {
            if (entities == null || entities.Count() == 0)
                return false;
            entities.AsParallel().ForAll(p => { dbSet.Attach(p); context.Entry(p).State = EntityState.Modified; });
            if (IsCommit)
                return await context.SaveChangesAsync() > 0;
            else
                return await Task.Run(() => false);
        }

        /// <summary>
        /// 删除列表
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public bool DeleteRange(IEnumerable<T> entities, bool IsCommit = true)
        {
            dbSet.RemoveRange(entities);
            if (IsCommit)
                return context.SaveChanges() > 0;
            else
                return false;
        }

        /// <summary>
        /// 删除列表
        /// </summary>
        /// <param name="entities">实体集合</param>
        /// <param name="IsCommit">是否提交</param>
        /// <returns></returns>
        public async Task<bool> DeleteRangeAsync(IEnumerable<T> entities, bool IsCommit = true)
        {
            dbSet.RemoveRange(entities);
            if (IsCommit)
                return await context.SaveChangesAsync() > 0;
            else
                return await Task.Run(() => false);
        }

        #endregion

        #region 获取集合数据
        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <returns></returns>
        public IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate)
        {
            return predicate != null ? dbSet.Where(predicate) : dbSet.AsEnumerable();
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            return predicate != null ? await Task.Factory.StartNew(() => dbSet.Where(predicate)) : await Task.Factory.StartNew(() => dbSet.AsEnumerable());
        }
        /// <summary>
        /// 查找(延时)
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <returns></returns>
        public IQueryable<T> FindAllDelay(Expression<Func<T, bool>> predicate)
        {
            return predicate != null ? dbSet.Where(predicate) : dbSet;
        }
        /// <summary>
        /// 查找(延时)
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <returns></returns>
        public async Task<IQueryable<T>> FindAllDelayAsync(Expression<Func<T, bool>> predicate)
        {
            return predicate != null ? await Task.Factory.StartNew(() => dbSet.Where(predicate)) : await Task.Factory.StartNew(() => dbSet);
        }
        #endregion

        #region 分页查找
        /// <summary>
        /// 分页查找
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <param name="pagingModel">分页模型</param>
        /// <returns></returns>
        public PagingModel<T> FindPaging(Expression<Func<T, bool>> predicate, PagingModel<T> pagingModel)
        {
            if (pagingModel == null)
                throw new NullReferenceException("PagingModel为NULL");
            if (pagingModel.PagingInfo == null)
                throw new NullReferenceException("PagingModel.PagingInfo为NULL");
            var totalItems = dbSet.LongCount();
            pagingModel.PagingInfo.TotalItems = totalItems;
            pagingModel.ModelList = dbSet.Where(predicate).Skip((Int32)((pagingModel.PagingInfo.CurrentPage - 1) * pagingModel.PagingInfo.ItemsPerPage)).Take((Int32)(pagingModel.PagingInfo.ItemsPerPage)).AsNoTracking();
            return pagingModel;
        }
        /// <summary>
        /// 分页查找
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <param name="pagingModel">分页模型</param>
        /// <returns></returns>
        public async Task<PagingModel<T>> FindPagingAsync(Expression<Func<T, bool>> predicate, PagingModel<T> pagingModel)
        {
            if (pagingModel == null)
                throw new NullReferenceException("PagingModel为NULL");
            if (pagingModel.PagingInfo == null)
                throw new NullReferenceException("PagingModel.PagingInfo为NULL");
            var totalItems = dbSet.LongCount();
            pagingModel.PagingInfo.TotalItems = totalItems;
            pagingModel.ModelList = await dbSet.Where(predicate).Skip((int)((pagingModel.PagingInfo.CurrentPage - 1) * pagingModel.PagingInfo.ItemsPerPage)).Take((int)(pagingModel.PagingInfo.ItemsPerPage)).AsNoTracking().ToListAsync();
            return pagingModel;
        }

        #endregion

        #region 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <returns></returns>
        public bool IsExist(Expression<Func<T, bool>> predicate)
        {
            return dbSet.Any(predicate);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="predicate">Lambda表达式</param>
        /// <returns></returns>
        public async Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate)
        {
            return await dbSet.AnyAsync(predicate);
        }

        #endregion

        #region SQL命令
        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="commandText">SQL命令</param>
        /// <param name="parameters">可选参数</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteSqlCommand(string commandText, params object[] parameters)
        {
            return context.Database.ExecuteSqlCommand(commandText, parameters);
        }
        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="commandText">SQL命令</param>
        /// <param name="parameters">可选参数</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteSqlCommandAsync(string commandText, params object[] parameters)
        {
            return await context.Database.ExecuteSqlCommandAsync(commandText, default(System.Threading.CancellationToken),parameters);
        }
        
        #endregion
    }
}
