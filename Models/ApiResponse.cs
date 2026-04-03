using System.Collections.Generic;

namespace FarmAPI.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public List<T> Data { get; set; } = new List<T>();
        public string Message { get; set; } = string.Empty;
    }

    public class SearchItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
