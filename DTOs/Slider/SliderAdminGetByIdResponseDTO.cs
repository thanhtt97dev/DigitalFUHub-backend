using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Slider
{
    public class SliderAdminGetByIdResponseDTO
    {
        public long SliderId { get; set; }
        public string Name { get; set; } = string.Empty!;
        public string Url { get; set; } = string.Empty!;
        public string Link { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
