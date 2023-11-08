const swiper = new Swiper("#homeMain .topSlider", {
  // Optional parameters
  loop: true,
  speed: 1200,
  spaceBetween: 200,

  autoplay: {
    delay: 4000,
  },

  effect: "fade",
  fadeEffect: {
    crossFade: true,
  },
});

const swiper2 = new Swiper("#homeMain .bottomSlider", {
  // Optional parameters
  loop: true,
  speed: 1600,
  spaceBetween: 200,

  autoplay: {
    delay: 4500,
  },

  effect: "fade",
  fadeEffect: {
    crossFade: true,
  },
});
