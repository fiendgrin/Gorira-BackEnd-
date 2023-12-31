﻿using Gorira.Models;

namespace Gorira.ViewModels.HomeVMs
{
    public class HomeVM
    {
        public Dictionary<string,string> Settings { get; set; }
        public IEnumerable<Slider> Sliders { get; set; }
        public IEnumerable<ReviewSlider> ReviewSliders { get; set; }
        public IEnumerable<Track>? TrendingTracks { get; set; }
        public IEnumerable<Genre>? TrendingGenres { get; set; }

        public IEnumerable<AppUser>? Users { get; set; }
    }
}
