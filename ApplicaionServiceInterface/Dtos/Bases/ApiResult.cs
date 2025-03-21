﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicaionServiceInterface.Dtos.Bases
{
    /// <summary>
    /// API请求回调基类
    /// </summary>
    public class ApiResult
    {
        public ApiResult()
        {

        }
        public ApiResult(string message = null, int code = 0, object data = null)
        {
            if (message != null)
                Message = message;
            if (code != 0)
                Code = code;
            if (data != null)
                Data = data;
        }
        /// <summary>
        /// 回调码=0正常其他异常
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 回调信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 回调Data
        /// </summary>
        public object Data { get; set; }
        public static ApiResult Ok(object data)
        {
            return new ApiResult
            {
                Code = 0,
                Message = "操作成功",
                Data = data
            };
        }
        public static ApiResult Ok()
        {
            return new ApiResult
            {
                Code = 0,
                Message = "操作成功",
                Data = true
            };
        }
        public static ApiResult Ok(bool data)
        {
            return new ApiResult
            {
                Code = 0,
                Message = "操作成功",
                Data = data
            };
        }
        public static ApiResult Err(string message = null, int code = -1)
        {
            return new ApiResult(message ?? "出错了,请稍后再试", code);
        }
        public static ApiResult<T> Ok<T>(T data)
        {
            return new ApiResult<T>
            {
                Code = 0,
                Data = data,
                Message = "操作成功"
            };
        }
    }
    /// <summary>
    /// API请求回调基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T>
    {
        /// <summary>
        /// 回调码=0正常其他异常
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 回调信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 回调Data
        /// </summary>
        public T Data { get; set; }
        public ApiResult(string message = null, int code = 0, T data = default)
        {
            if (message != null)
                Message = message;
            if (code != 0)
                Code = code;
            this.Data = data;
        }
    }
}
