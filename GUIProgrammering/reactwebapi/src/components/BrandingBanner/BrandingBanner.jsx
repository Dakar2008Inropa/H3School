import React from "react";
import PropTypes from "prop-types";
import "./BrandingBanner.css";

function toRgba(color, alpha) {
    const rgbMatch = /^rgba?\(([^)]+)\)$/i.exec(color);
    if (rgbMatch) {
        const parts = rgbMatch[1].split(",").map((p) => p.trim());
        const r = Number.parseInt(parts[0], 10);
        const g = Number.parseInt(parts[1], 10);
        const b = Number.parseInt(parts[2], 10);
        return `rgba(${r}, ${g}, ${b}, ${alpha})`;
    }

    if (color.startsWith("#")) {
        let r;
        let g;
        let b;

        if (color.length === 4) {
            r = Number.parseInt(color[1] + color[1], 16);
            g = Number.parseInt(color[2] + color[2], 16);
            b = Number.parseInt(color[3] + color[3], 16);
        } else if (color.length === 7) {
            r = Number.parseInt(color.slice(1, 3), 16);
            g = Number.parseInt(color.slice(3, 5), 16);
            b = Number.parseInt(color.slice(5, 7), 16);
        } else {
            return `rgba(0, 0, 0, ${alpha})`;
        }

        return `rgba(${r}, ${g}, ${b}, ${alpha})`;
    }
    return `rgba(0, 0, 0, ${alpha})`;
}

export const BrandingBanner = ({
    imageUrl,
    overlayColor = "#2f6f73",
    overlayOpacity = 0.85,
    overlayWidthPercent = 45,
    title = "Fashion starts here.",
    subtitle = "Discover refined design, soft tones, and the art of subtle sophistication.",
    highlight = "Only at AURA.",
    ariaLabel = "Promotional fashion banner",
    overlaySide = "left",
}) => {
    const overlayBg = toRgba(overlayColor, overlayOpacity);
    const overlaySideClass =
        overlaySide === "right"
            ? "branding-banner__overlay--right"
            : "branding-banner__overlay--left";
    return (

        <section
            className="branding-banner"
            aria-label={ariaLabel}
            style={{
                backgroundImage: imageUrl ? `url("${imageUrl}")` : "none",
            }}
        >
            <div
                className={`branding-banner__overlay ${overlaySideClass}`}
                style={{
                    width: `${overlayWidthPercent}%`,
                    backgroundColor: overlayBg,
                }}
            >
                <div className="branding-banner__content">
                    <h2 className="branding-banner__title">{title}</h2>
                    <p className="branding-banner__subtitle">{subtitle}</p>
                    <p className="branding-banner__highlight">{highlight}</p>
                </div>
            </div>
        </section>
    );
};

BrandingBanner.propTypes = {
    imageUrl: PropTypes.string,
    overlayColor: PropTypes.string,
    overlayOpacity: PropTypes.number,
    overlayWidthPercent: PropTypes.number,
    title: PropTypes.string,
    subtitle: PropTypes.string,
    highlight: PropTypes.string,
    ariaLabel: PropTypes.string,
    overlaySide: PropTypes.oneOf(["left", "right"])
};