using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    /// <summary>
    /// Generic service response wrapper for API/Service calls
    /// </summary>
    public class ServiceResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Create a successful response
        /// </summary>
        public static ServiceResponse<T> Success(T data, string message = "Operation successful")
        {
            return new ServiceResponse<T>
            {
                Succeeded = true,
                Data = data,
                Message = message
            };
        }

        /// <summary>
        /// Create a failed response
        /// </summary>
        public static ServiceResponse<T> Failure(string message, List<string> errors = null)
        {
            return new ServiceResponse<T>
            {
                Succeeded = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        /// <summary>
        /// Create a failed response with multiple errors
        /// </summary>
        public static ServiceResponse<T> Failure(List<string> errors)
        {
            return new ServiceResponse<T>
            {
                Succeeded = false,
                Message = "Operation failed with one or more errors",
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Non-generic service response for operations that don't return data
    /// </summary>
    public class ServiceResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static ServiceResponse Success(string message = "Operation successful")
        {
            return new ServiceResponse
            {
                Succeeded = true,
                Message = message
            };
        }

        public static ServiceResponse Failure(string message, List<string> errors = null)
        {
            return new ServiceResponse
            {
                Succeeded = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ServiceResponse Failure(List<string> errors)
        {
            return new ServiceResponse
            {
                Succeeded = false,
                Message = "Operation failed with one or more errors",
                Errors = errors
            };
        }
    }
}
