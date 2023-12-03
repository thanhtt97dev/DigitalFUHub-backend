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
        List<Slider> GetSliders();
        List<Slider> GetSliders(int statusActive, int page);
        int GetNumberSliderByConditions(int statusActive);
        void AddSlider(Slider slider);
        Slider? GetSliderById(long sliderId);
        void UpdateSlider(Slider newSlider);
        void UpdateStatusActiveSlider(long sliderId, bool newStatusActive);
        void DeleteSlider(Slider slider);
    }
}
