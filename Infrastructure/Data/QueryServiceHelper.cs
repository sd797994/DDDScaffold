﻿using Microsoft.EntityFrameworkCore;

namespace InfrastructureBase.Data
{
    public class QueryServiceHelper
    {
        public static async Task<(List<T> Data, int Total)> PageQuery<T>(IQueryable<T> query, int page, int limit) where T : class
        {
            var total = await query.CountAsync();
            var data = await query.Skip((page - 1) * limit).Take(limit).AsNoTracking().ToListAsync();
            return (data, total);
        }
    }
}
