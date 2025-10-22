import { useMemo, useState } from "react";
import "./Carousel.css";

export const Carousel = () => {
  const MAX_IMAGES = 9;

  const images = useMemo(
    () => Array.from({ length: MAX_IMAGES }, (_, i) => `/images/p${i + 1}.jpg`),
    [MAX_IMAGES]
  );

  const [index, setIndex] = useState(0);
  const [effect, setEffect] = useState("fade");

  function goNext() {
    setIndex((i) => (i + 1) % images.length);
  }

  function goPrev() {
    setIndex((i) => (i - 1 + images.length) % images.length);
  }

  function goTo(i) {
    setIndex(((i % images.length) + images.length) % images.length);
  }

  return (
    <section className="carousel" aria-label="Image carousel">
      <div className="carousel__toolbar">
        <label className="carousel__label" htmlFor="transitionSelect">
          Transition
        </label>
        <select
          id="transitionSelect"
          className="carousel__select"
          value={effect}
          onChange={(e) => setEffect(e.target.value)}
        >
          <option value="fade">Fade</option>
          <option value="slide">Slide</option>
        </select>
      </div>
      <div
        className={
          effect === "slide"
            ? "carousel__viewport carousel__viewport--slide"
            : "carousel__viewport carousel__viewport--fade"
        }
      >
        <div
          className="carousel__track"
          style={
            effect === "slide"
              ? { transform: `translateX(-${index * 100}%)` }
              : undefined
          }
        >
          {images.map((src, i) => (
            <div
              key={src}
              className={
                "carousel__slide" +
                (effect === "fade" ? (i === index ? " is-active" : "") : "")
              }
              aria-roledescription="slide"
              aria-label={`${i + 1} of ${images.length}`}
            >
              <img
                className="carousel__img"
                src={src}
                alt={`${i + 1}`}
                draggable="false"
              />
            </div>
          ))}
        </div>
      </div>
      <div className="carousel__controls">
        <button
          type="button"
          className="carousel__btn"
          onClick={goPrev}
          aria-label="Previous"
        >
          ‹
        </button>
        <div className="carousel__dots" role="tablist" aria-label="Slides">
          {images.map((_, i) => (
            <button
              key={i}
              type="button"
              className={"carousel__dot" + (i === index ? " is-active" : "")}
              aria-label={`Go to slide ${i + 1}`}
              aria-selected={i === index}
              role="tab"
              onClick={() => goTo(i)}
            />
          ))}
        </div>
        <button
          type="button"
          className="carousel__btn"
          onClick={goNext}
          aria-label="Next"
        >
          ›
        </button>
      </div>
    </section>
  );
};
