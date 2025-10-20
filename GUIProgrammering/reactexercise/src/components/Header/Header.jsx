import PropTypes from 'prop-types';
import './Header.css';

export const Header = ({ name = 'Your Name' }) => {
    return (
        <>
            <header className="header-container" role="banner" aria-label="Site header">
                <h1 className="header-title" title={name}>
                    {name}
                </h1>
            </header>
            <div className="header-spacer" />
        </>
    );
};

Header.propTypes = {
    name: PropTypes.string
};