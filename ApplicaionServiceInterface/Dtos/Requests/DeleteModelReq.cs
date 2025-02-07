using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicaionServiceInterface.Dtos.Requests
{
    /// <summary>
    /// 通用删除实体入参
    /// </summary>
    public class DeleteModelReq
    {
        public int[] IdLists { get; set; }
    }
}
