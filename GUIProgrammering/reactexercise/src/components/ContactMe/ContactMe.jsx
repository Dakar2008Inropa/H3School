import { useState } from "react";
import "./ContactMe.css";

export const ContactMe = () => {
    const [status, setStatus] = useState("");

    function handleSubmit(event) {
        event.preventDefault();
        const form = event.currentTarget;
        const data = new FormData(form);

        const name = data.get("name");
        const email = data.get("email");
        const message = data.get("message");

        console.log("Contact form submitted:", { name, email, message });
        setStatus("Your message has been sent. Thank you!");
        form.reset();

        window.setTimeout(() => setStatus(""), 4000);
    }

    return (
        <article className="contact-card" aria-labelledby="contact-title">
            <h2 id="contact-title" className="contact-card__title">Contact Me</h2>
            <p className="contact-card__intro">
                Feel free to reach out using the form below.
            </p>

            <form className="contact-card__form" onSubmit={handleSubmit}>
                <div className="contact-card__row">
                    <label htmlFor="name" className="contact-card__label">Name</label>
                    <input
                        id="name"
                        name="name"
                        type="text"
                        className="contact-card__input"
                        placeholder="Your name"
                        required
                    />
                </div>

                <div className="contact-card__row">
                    <label htmlFor="email" className="contact-card__label">Email</label>
                    <input
                        id="email"
                        name="email"
                        type="email"
                        className="contact-card__input"
                        placeholder="you@example.com"
                        required
                    />
                </div>

                <div className="contact-card__row">
                    <label htmlFor="message" className="contact-card__label">Message</label>
                    <textarea
                        id="message"
                        name="message"
                        className="contact-card__input contact-card__textarea"
                        placeholder="How can I help you?"
                        rows={5}
                        required
                    />
                </div>

                <div className="contact-card__actions">
                    <button type="submit" className="contact-card__button">Send Message</button>
                </div>

                {status && (
                    <p className="contact-card__status" role="status" aria-live="polite">
                        {status}
                    </p>
                )}
            </form>
        </article>
    );
};