namespace HIS.Models
{
    /// <summary>
    /// 统一API响应格式
    /// </summary>
    public class ApiResult
    {
        /// <summary>状态码 0:成功 1:失败</summary>
        public int Code { get; set; }

        /// <summary>提示信息</summary>
        public string Msg { get; set; } = string.Empty;

        /// <summary>返回数据</summary>
        public object? Data { get; set; }

        public static ApiResult Success(string msg = "操作成功", object? data = null)
        {
            return new ApiResult { Code = 0, Msg = msg, Data = data };
        }

        public static ApiResult Fail(string msg = "操作失败")
        {
            return new ApiResult { Code = 1, Msg = msg };
        }
    }

    /// <summary>
    /// 统一API响应格式(泛型)
    /// </summary>
    public class ApiResult<T>
    {
        public int Code { get; set; }
        public string Msg { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResult<T> Success(string msg = "操作成功", T? data = default)
        {
            return new ApiResult<T> { Code = 0, Msg = msg, Data = data };
        }

        public static ApiResult<T> Fail(string msg = "操作失败")
        {
            return new ApiResult<T> { Code = 1, Msg = msg };
        }
    }

    /// <summary>
    /// Layui Table 专用响应格式
    /// </summary>
    public class LayuiTableResult
    {
        /// <summary>状态码 0:成功</summary>
        public int Code { get; set; } = 0;

        /// <summary>提示信息</summary>
        public string Msg { get; set; } = string.Empty;

        /// <summary>总记录数</summary>
        public int Total { get; set; }

        /// <summary>数据列表</summary>
        public object? Data { get; set; }

        public static LayuiTableResult Ok(int total, object data)
        {
            return new LayuiTableResult { Code = 0, Msg = "", Total = total, Data = data };
        }
    }
}
