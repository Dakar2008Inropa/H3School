import PropTypes from 'prop-types';
import './GridContainer.css';

export const GridContainer = ({ children }) => {
    return (
        <div className="grid-container">
            {children}
        </div>
    );
};

GridContainer.propTypes = {
    children: PropTypes.node
};