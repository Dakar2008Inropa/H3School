import PropTypes from "prop-types";
import "./Offer.css";

export const Offer = ({
    mainTitle = "AUTUMN DEALS",
    saveTitle = "Save up to 70%",
    imageSrc = "https://images.unsplash.com/photo-1512436991641-6745cdb1723f?q=80&w=1600&auto=format&fit=crop",
    imageAlt = "Offer image"
}) => {
    return (
        <aside className="offer" role="complementary" aria-label="Current offer">
            <div className="offer__inner">
                <div className="offer__copy">
                    <h2 className="offer__title">{mainTitle}</h2>
                    <p className="offer__subtitle">{saveTitle}</p>
                </div>
                <div className="offer__imageWrap">
                    <img className="offer__image" src={imageSrc} alt={imageAlt} loading="lazy" />
                </div>
            </div>
        </aside>
    );
};

Offer.propTypes = {
    mainTitle: PropTypes.string,
    saveTitle: PropTypes.string,
    imageSrc: PropTypes.string,
    imageAlt: PropTypes.string
};