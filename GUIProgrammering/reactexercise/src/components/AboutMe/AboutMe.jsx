import "./AboutMe.css";

export const AboutMe = () => {
    return (
        <article className="about-card" aria-labelledby="about-title">
            <div className="about-card__media">
                <img
                    className="about-card__image"
                    src="/images/me.jpg"
                    alt="Portrait of Daniel Vinther Andersen"
                    loading="lazy"
                />
            </div>
            <div className="about-card__content">
                <h2 id="about-title" className="about-card__title">About Me</h2>
                <p className="about-card__text">
                    Hi, I’m Daniel Vinther Andersen. I enjoy building modern, accessible user interfaces and learning new technologies.
                    This site showcases a few UI components such as a carousel, counter, and a simple product grid.
                </p>
                <ul className="about-card__bullets" aria-label="Highlights">
                    <li>Focused on clean, readable code</li>
                    <li>Passionate about UX and performance</li>
                    <li>Curious and constantly improving</li>
                </ul>
            </div>
        </article>
    );
};