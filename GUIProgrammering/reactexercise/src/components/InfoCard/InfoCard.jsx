import PropTypes from 'prop-types';
import './InfoCard.css';

const defaultHobbies = ['Coding', 'Photography', 'Running'];

export const InfoCard = ({
    name = 'Your Name',
    imageSrc = '/images/me.jpg',
    imageAlt = 'Portrait of me',
    hobbies = defaultHobbies,
}) => {
    return (
        <div className="info-card-center">
            <section
                className="info-card"
                aria-labelledby="info-card-title"
            >
                <div className="info-card__image-wrap">
                    <img
                        className="info-card__image"
                        src={imageSrc}
                        alt={imageAlt}
                        loading="lazy"
                    />
                </div>
                <div className="info-card__content">
                    <h2 id="info-card-title" className="info-card__title">
                        {name}
                    </h2>

                    <h3 className="info-card__subtitle">My Hobbies</h3>
                    <ul className="info-card__list">
                        {hobbies.slice(0, 3).map((hobby, idx) => (
                            <li key={`${hobby}-${idx}`} className="info-card__list-item">
                                {hobby}
                            </li>
                        ))}
                    </ul>
                </div>
            </section>
        </div>
    );
};

InfoCard.propTypes = {
    name: PropTypes.string,
    imageSrc: PropTypes.string,
    imageAlt: PropTypes.string,
    hobbies: PropTypes.arrayOf(PropTypes.string),
};