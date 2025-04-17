using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Models
{
    public class ApiResult<T>
    {
        public bool Succeeded { get; set; }
        public T? Result { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResult<T> Success(T data) => new() { Succeeded = true, Result = data };
        public static ApiResult<T> Fail(params string[] errors) => new() { Succeeded = false, Errors = errors.ToList() };
    }
}
