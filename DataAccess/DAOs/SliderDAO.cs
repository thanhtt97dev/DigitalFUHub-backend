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

        internal List<Slider> GetSliders (int statusActive, int page)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var result = (from slider in context.Sliders
                              where 
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

        internal Slider? GetSliderById (long sliderId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var slider = context.Sliders.FirstOrDefault(x => x.SliderId == sliderId);

                return slider;
            }
        }

        internal int GetNumberSliderByConditions(int statusActive)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var query = (from slider in context.Sliders
                             where (statusActive == Constants.STATUS_ALL_SLIDER_FOR_FILTER ? true 
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
                context.Sliders.Update(newSlider);
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

        internal void DeleteSlider (Slider slider)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                context.Sliders.Remove(slider);
                context.SaveChanges();
            }
        }
    }
}
