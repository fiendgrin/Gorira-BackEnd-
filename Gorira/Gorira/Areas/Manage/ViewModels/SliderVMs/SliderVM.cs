using Gorira.Models;

namespace Gorira.Areas.Manage.ViewModels.SliderVMs
{
    public class SliderVM
    {
        public IEnumerable<Slider>? Sliders { get; set; }
        public IEnumerable<ReviewSlider>? ReviewSliders { get; set; }
    }
}
