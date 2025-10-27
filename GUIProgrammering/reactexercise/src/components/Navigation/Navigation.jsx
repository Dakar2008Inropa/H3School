import { NavLink } from "react-router-dom";
import "./Navigation.css";

const pageModules = import.meta.glob("../../pages/*.jsx");

const navItems = Object.keys(pageModules)
    .map(p => {
        const match = p.match(/\/pages\/(.+)\.jsx$/);
        const name = match ? match[1] : "";
        const slug = name.toLowerCase();
        if (!name || slug === "home") return null;
        return { path: `/${slug}`, label: name };
    })
    .filter(Boolean)
    .sort((a, b) => a.label.localeCompare(b.label));

export const Navigation = () => {
    return (
        <>
            <nav className="nav" role="navigation" aria-label="Main">
                <div className="nav__inner">
                    <ul className="nav__list">
                        <li className="nav__item">
                            <NavLink
                                to="/"
                                end
                                className={({ isActive }) => "nav__link" + (isActive ? " is-active" : "")}
                            >
                                Home
                            </NavLink>
                        </li>
                        {navItems.map(item => (
                            <li key={item.path} className="nav__item">
                                <NavLink
                                    to={item.path}
                                    className={({ isActive }) => "nav__link" + (isActive ? " is-active" : "")}
                                >
                                    {item.label}
                                </NavLink>
                            </li>
                        ))}
                    </ul>
                </div>
            </nav>
            <div className="nav-spacer" />
        </>
    );
};