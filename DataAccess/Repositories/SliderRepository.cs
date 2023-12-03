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

        public void DeleteSlider(Slider slider) => SliderDAO.Instance.DeleteSlider(slider);

        public int GetNumberSliderByConditions(int statusActive) 
            => SliderDAO.Instance.GetNumberSliderByConditions(statusActive);

        public Slider? GetSliderById(long sliderId) => SliderDAO.Instance.GetSliderById(sliderId);

        public List<Slider> GetSliders(int statusActive, int page) 
            => SliderDAO.Instance.GetSliders(statusActive, page);

        public List<Slider> GetSliders() => SliderDAO.Instance.GetSliders();

        public void UpdateSlider(Slider newSlider) => SliderDAO.Instance.UpdateSlider(newSlider);

        public void UpdateStatusActiveSlider(long sliderId, bool newStatusActive) => SliderDAO.Instance.UpdateStatusActiveSlider(sliderId, newStatusActive);
    }
}
