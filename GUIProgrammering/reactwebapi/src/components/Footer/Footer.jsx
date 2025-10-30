import { useEffect, useState, useCallback } from "react";
import './Footer.css';

function isValidEmail(value) {
    const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;
    return pattern.test(value.trim());
}

function setCookie(name, value, days) {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = name + "=" + encodeURIComponent(value) + "; expires=" + expires + "; path=/; SameSite=Lax";
}

function getCookie(name) {
    const part = document.cookie
        .split("; ")
        .find((row) => row.startsWith(name + "="));
    if (!part) {
        return null;
    }
    const value = part.split("=")[1];
    return decodeURIComponent(value);
}

function deleteCookie(name) {
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; SameSite=Lax";
}

function maskEmail(email) {
    const at = email.indexOf("@");
    if (at <= 1) {
        return email;
    }
    return email[0] + "***" + email.substring(at - 1);
}

function handleDummyLinkClick(e) {
    e.preventDefault();
}

export const Footer = () => {
    const [email, setEmail] = useState("");
    const [message, setMessage] = useState(null);
    const [subscribed, setSubscribed] = useState(false);
    const [inputHasError, setInputHasError] = useState(false);

    useEffect(() => {
        const stored = getCookie("newsletter_email");
        if (stored) {
            setSubscribed(true);
            setMessage({
                type: "success",
                text: `You are already subscribed with ${maskEmail(stored)}.`,
            });
        }
    }, []);

    const handleKeyDown = useCallback(
        (e) => {
            if (e.key !== "Enter") {
                return;
            }
            e.preventDefault();

            if (!isValidEmail(email)) {
                setInputHasError(true);
                setMessage({ type: "error", text: "Please enter a valid email address." });
                return;
            }

            setCookie("newsletter_email", email.trim(), 365);
            setSubscribed(true);
            setInputHasError(false);
            setMessage({ type: "success", text: "You are subscribed to the newsletter." });
        },
        [email]
    );

    const handleChange = useCallback((e) => {
        setEmail(e.target.value);
        if (inputHasError) {
            setInputHasError(false);
            if (message?.type === "error") {
                setMessage(null);
            }
        }
    }, [inputHasError, message]);

    const handleResetSubscription = useCallback(() => {
        deleteCookie("newsletter_email");
        setSubscribed(false);
        setEmail("");
        setMessage({ type: "success", text: "You can subscribe again." });
    }, []);

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
                        {subscribed && (
                            <button
                                type="button"
                                className="newsletter-reset"
                                onClick={handleResetSubscription}
                                aria-label="Reset newsletter subscription for testing"
                            >
                                Reset newsletter subscription (dev)
                            </button>
                        )}
                        {message && (
                            <div
                                className={`newsletter-message ${message.type}`}
                                aria-live="polite"
                            >
                                {message.text}
                            </div>
                        )}
                        <div className="newsletter-input">
                            <input
                                type="email"
                                inputMode="email"
                                name="newsletter"
                                placeholder={subscribed ? "Already subscribed" : "Enter your email and press Enter"}
                                value={email}
                                onChange={handleChange}
                                onKeyDown={handleKeyDown}
                                disabled={subscribed}
                                aria-invalid={inputHasError}
                                aria-describedby={inputHasError ? "newsletter-error" : undefined}
                                className={inputHasError ? "error" : undefined}
                                autoComplete="email"
                            />
                        </div>
                    </section>
                </div>
            </div>
        </footer>
    );
}