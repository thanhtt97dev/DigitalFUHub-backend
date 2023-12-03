using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataAccess.Repositories
{
    public class SliderRepository : ISliderRepository
    {
        public void AddSlider(Slider slider) => SliderDAO.Instance.AddSlider(slider);

        public void DeleteSlider(long sliderId) => SliderDAO.Instance.DeleteSlider(sliderId);

        public int GetNumberSliderByConditions(string name, string link, DateTime? startDate, DateTime? endDate, int statusActive) 
            => SliderDAO.Instance.GetNumberSliderByConditions(name, link, startDate,endDate, statusActive);

        public Slider? GetSliderById(long sliderId) => SliderDAO.Instance.GetSliderById(sliderId);

        public List<Slider> GetSliders(string name, string link, DateTime? startDate, DateTime? endDate, int statusActive, int page) 
            => SliderDAO.Instance.GetSliders(name, link, startDate, endDate, statusActive, page);

        public void UpdateSlider(Slider newSlider) => SliderDAO.Instance.UpdateSlider(newSlider);

        public void UpdateStatusActiveSlider(long sliderId, bool newStatusActive) => SliderDAO.Instance.UpdateStatusActiveSlider(sliderId, newStatusActive);
    }
}
