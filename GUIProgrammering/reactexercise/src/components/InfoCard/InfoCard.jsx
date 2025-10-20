import { useEffect, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import './InfoCard.css';

const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

const defaultHobbies = ['Coding', 'Photography', 'Running'];

export const InfoCard = ({
    name = 'Your Name',
    imageSrc = '/images/me.jpg',
    imageAlt = 'Portrait of me',
    hobbies = defaultHobbies,
}) => {
    const centerRef = useRef(null);
    const cardRef = useRef(null);
    const titleRef = useRef(null);

    const [offset, setOffset] = useState({ x: 0, y: 0 });

    const handleTitleMouseMove = (e) => {
        if (!centerRef.current || !cardRef.current || !titleRef.current) {
            return;
        }

        if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            return;
        }

        const containerRect = centerRef.current.getBoundingClientRect();
        const cardRect = cardRef.current.getBoundingClientRect();
        const titleRect = titleRef.current.getBoundingClientRect();

        const maxX = Math.max(0, (containerRect.width - cardRect.width) / 2);
        const maxY = Math.max(0, (containerRect.height - cardRect.height) / 2);

        const cx = titleRect.left + (titleRect.width / 2);
        const cy = titleRect.top + (titleRect.height / 2);
        const nx = clamp((e.clientX - cx) / (titleRect.width / 2), -1, 1);
        const ny = clamp((e.clientY - cy) / (titleRect.height / 2), -1, 1);

        const x = nx * maxX;
        const y = ny * maxY;

        setOffset({ x, y });
    };

    const resetPosition = () => setOffset({ x: 0, y: 0 });

    useEffect(() => {
        const onResize = () => {
            if (!centerRef.current || !cardRef.current) {
                return;
            }
            const containerRect = centerRef.current.getBoundingClientRect();
            const cardRect = cardRef.current.getBoundingClientRect();
            const maxX = Math.max(0, (containerRect.width - cardRect.width) / 2);
            const maxY = Math.max(0, (containerRect.height - cardRect.height) / 2);

            setOffset((prev) => ({
                x: clamp(prev.x, -maxX, maxX),
                y: clamp(prev.y, -maxY, maxY),
            }));
        };

        window.addEventListener('resize', onResize);
        return () => window.removeEventListener('resize', onResize);
    }, []);

    return (
        <div className="info-card-center" ref={centerRef}>
            <section
                className="info-card"
                aria-labelledby="info-card-title"
                style={{ transform: `translate(${offset.x}px, ${offset.y}px)` }}
                ref={cardRef}
            >
                <div className="info-card__image-wrap">
                    <img
                        className="info-card__image"
                        src={imageSrc}
                        alt={imageAlt}
                        loading="lazy"
                        onMouseEnter={resetPosition}
                    />
                </div>
                <div className="info-card__content">
                    <h2
                        id="info-card-title"
                        className="info-card__title"
                        ref={titleRef}
                        onMouseMove={handleTitleMouseMove}
                    >
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
    hobbies: PropTypes.arrayOf(PropTypes.string)
};