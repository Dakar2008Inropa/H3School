import './Footer.css';

function handleDummyLinkClick(e) {
    e.preventDefault();
}

export const Footer = () => {
    return (
        <footer className="footer">
            <div className="footer-inner">
                <div className="footer-grid">
                    <nav className="footer-col" aria-labelledby="footer-customer-service">
                        <h3 id="footer-customer-service" className="footer-col-title">
                            Customer Service
                        </h3>
                        <ul className="footer-links">
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>FAQ</a>
                            </li>
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>Delivery</a>
                            </li>
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>Privacy Policy</a>
                            </li>
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>Cookies</a>
                            </li>
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>Contact us</a>
                            </li>
                        </ul>
                    </nav>
                    <nav className="footer-col" aria-labelledby="footer-about-aura">
                        <h3 id="footer-about-aura" className="footer-col-title">
                            About AURA
                        </h3>
                        <ul className="footer-links">
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>About the company</a>
                            </li>
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>Job &amp; career</a>
                            </li>
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>Newsletter</a>
                            </li>
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>Stores</a>
                            </li>
                            <li>
                                <a href="#" onClick={handleDummyLinkClick}>Responsibility</a>
                            </li>
                        </ul>
                    </nav>
                    <section className="newsletter-col" aria-labelledby="newsletter-title">
                        <h2 id="newsletter-title" className="newsletter-title">Stay in the Aura.</h2>
                        <p className="newsletter-subtitle">
                            Join our newsletter and get <strong>15% off your first order.</strong>
                        </p>
                        <p className="newsletter-copy">
                            Be the first to know about new drops, exclusive offers, and timeless minimalist pieces that actually belong in your wardrobe.
                        </p>
                        <p className="newsletter-fineprint">
                            No spam. No clutter. Just clean style - the Aura way.
                        </p>
                        <div className="newsletter-input">
                            <input
                                type="email"
                                placeholder="Your email"
                                aria-label="Your email"
                            />
                        </div>
                    </section>
                </div>
            </div>
        </footer>
    );
}