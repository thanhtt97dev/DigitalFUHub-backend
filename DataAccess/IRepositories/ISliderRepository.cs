using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface ISliderRepository
    {
        List<Slider> GetSliders(string name, string link, DateTime? startDate, DateTime? endDate, int statusActive, int page);
        int GetNumberSliderByConditions(string name, string link, DateTime? startDate, DateTime? endDate, int statusActive);
        void AddSlider(Slider slider);
        Slider? GetSliderById(long sliderId);
        void UpdateSlider(Slider newSlider);
        void UpdateStatusActiveSlider(long sliderId, bool newStatusActive);
        void DeleteSlider(long sliderId);
    }
}
