import PropTypes from 'prop-types';
import './Footer.css';

function parseBirthDDMMYY(birth) {
    if (typeof birth !== 'string' || birth.length !== 6 || !/^\d{6}$/.test(birth)) {
        return null;
    }

    const day = Number(birth.slice(0, 2));
    const month = Number(birth.slice(2, 4));
    const yy = Number(birth.slice(4, 6));

    const now = new Date();
    const currentYY = now.getFullYear() % 100;
    const fullYear = yy <= currentYY ? 2000 + yy : 1900 + yy;

    if (day < 1 || day > 31 || month < 1 || month > 12) {
        return null;
    }
    const date = new Date(fullYear, month - 1, day);

    if (
        date.getFullYear() !== fullYear ||
        date.getMonth() !== month - 1 ||
        date.getDate() !== day
    ) {
        return null;
    }

    return date;
}

function calculateAge(birthDate) {
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();

    const hasHadBirthdayThisYear =
        today.getMonth() > birthDate.getMonth() ||
        (today.getMonth() === birthDate.getMonth() && today.getDate() >= birthDate.getDate());

    if (!hasHadBirthdayThisYear) {
        age -= 1;
    }

    return age;
}

const Footer = ({ birth = '010195' }) => {
    const birthDate = parseBirthDDMMYY(birth);
    const age = birthDate ? calculateAge(birthDate) : null;

    return (
        <>
            <footer className="footer-container" role="contentinfo" aria-label="Site footer">
                <p className="footer-text">
                    {age !== null ? `Age: ${age}` : 'Invalid birthdate'}
                </p>
            </footer>
            <div className="footer-spacer" />
        </>
    );
};

Footer.propTypes = {
    birth: PropTypes.string,
};

export default Footer;