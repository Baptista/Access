﻿using ClientAcess.Models.Enums;

namespace ClientAcess.Models
{
    public class ApiResponse<T>
    {
        public ApiCode Status { get; set; }
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public T? Response { get; set; }
    }
}
