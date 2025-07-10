using System;

namespace PrjEtax.Models    // <-- đổi thành namespace thực của bạn nếu khác
{
    public class ErrorViewModel
    {
        /// <summary>
        /// ID của request (tự động được ASP.NET gán khi có lỗi)
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Nếu có RequestId thì hiển thị, ngược lại ẩn
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
