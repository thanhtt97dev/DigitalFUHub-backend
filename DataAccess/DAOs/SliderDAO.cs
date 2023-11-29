using BusinessObject;
using BusinessObject.Entities;
using Comons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
    internal class SliderDAO
    {
        private static SliderDAO? instance;
        private static readonly object instanceLock = new object();
        public static SliderDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new SliderDAO();
                    }
                }
                return instance;
            }
        }

        internal List<Slider> GetSliders (string name, string link, DateTime? startDate, DateTime? endDate, int statusActive, int page)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var result = (from slider in context.Sliders
                              where (string.IsNullOrEmpty(name) ? true : slider.Name.Trim().ToUpper().Equals(name.Trim().ToUpper()))
                              &&
                              (string.IsNullOrEmpty(link) ? true : slider.Link.Trim().ToUpper().Equals(link.Trim().ToUpper()))
                              &&
                              (startDate == null ? true : slider.DateCreate >= startDate)
                              &&
                              (endDate == null ? true : slider.DateCreate <= endDate)
                              &&
                             (statusActive == Constants.STATUS_ALL_SLIDER_FOR_FILTER ? true
                             : (statusActive == Constants.STATUS_ACTIVE_SLIDER_FOR_FILTER ? slider.IsActive == true : slider.IsActive == false))
                             select new Slider
                              {
                                  SliderId = slider.SliderId,
                                  Name = slider.Name,
                                  Link = slider.Link,
                                  DateCreate = slider.DateCreate,
                                  Url = slider.Url,
                                  IsActive = slider.IsActive,
                              }).Skip((page - 1) * Constants.PAGE_SIZE_SLIDER)
                                         .Take(Constants.PAGE_SIZE_SLIDER)
                                         .ToList();

                return result;
            }
        }

        internal int GetNumberSliderByConditions(string name, string link, DateTime? startDate, DateTime? endDate, int statusActive)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var query = (from slider in context.Sliders
                             where (string.IsNullOrEmpty(name) ? true : slider.Name.Trim().ToUpper().Equals(name.Trim().ToUpper()))
                             &&
                             (string.IsNullOrEmpty(link) ? true : slider.Link.Trim().ToUpper().Equals(link.Trim().ToUpper()))
                             &&
                             (startDate == null ? true : slider.DateCreate >= startDate)
                             &&
                             (endDate == null ? true : slider.DateCreate <= endDate)
                             &&
                             (statusActive == Constants.STATUS_ALL_SLIDER_FOR_FILTER ? true 
                             : (statusActive == Constants.STATUS_ACTIVE_SLIDER_FOR_FILTER ? slider.IsActive == true : slider.IsActive == false))
                             select new Slider {});

                return query.Count();
            }
        }

        internal void AddSlider(Slider slider)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                context.Sliders.Add(slider);
                context.SaveChanges();
            }
        }

        internal void UpdateSlider (Slider newSlider)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var sliderFind = context.Sliders.FirstOrDefault(x => x.SliderId == newSlider.SliderId);
                if (sliderFind == null) throw new ArgumentNullException("Slider not found");

                // update slider
                sliderFind.Name = newSlider.Name;
                sliderFind.Link = newSlider.Link;
                sliderFind.Url = newSlider.Url;
                sliderFind.IsActive = newSlider.IsActive;

                context.SaveChanges();
            }
        }

        internal void UpdateStatusActiveSlider(long sliderId, bool newStatusActive)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var sliderFind = context.Sliders.FirstOrDefault(x => x.SliderId == sliderId);
                if (sliderFind == null) throw new ArgumentNullException("Slider not found");

                // update status active slider
                sliderFind.IsActive = newStatusActive;

                context.SaveChanges();
            }
        }


        internal void DeleteSlider (long sliderId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var sliderFind = context.Sliders.FirstOrDefault(x => x.SliderId == sliderId);
                if (sliderFind == null) throw new ArgumentNullException("Slider not found");

                // update status active slider
                context.Sliders.Remove(sliderFind);
                context.SaveChanges();
            }
        }




    }
}
