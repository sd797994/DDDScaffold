using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicaionServiceInterface.Dtos.Bases
{
    public class PageQueryResonseBase<T>
    {
        public PageQueryResonseBase(List<T> data, int total)
        {
            this.List = data;
            this.Total = total;
        }
        public List<T> List { get; set; }
        public int Total { get; set; }
    }
}
