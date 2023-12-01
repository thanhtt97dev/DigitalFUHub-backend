using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Slider
{
    public class SliderAdminRequestParamDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StatusActive { get; set; }
        public int Page { get; set; }
    }
}
