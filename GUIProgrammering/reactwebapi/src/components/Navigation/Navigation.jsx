import { useEffect, useState } from "react";
import { NavLink, useLocation, useSearchParams } from "react-router-dom";
import { CategoriesApi } from "../../api/categoriesApi";
import "./Navigation.css";

export const Navigation = () => {
    const [categories, setCategories] = useState([]);

    useEffect(() => {
        const ctrl = new AbortController();
        async function load() {
            try {
                const data = await CategoriesApi.list(ctrl.signal);

                const items = Array.isArray(data) ? data.slice().sort((a, b) => Number(a.categoryId) - Number(b.categoryId)) : [];
                setCategories(items);
            }
            catch (e) {
                console.error("Failed to load categories for navigation:", e);
                setCategories([]);
            }
        }
        load();
        return () => ctrl.abort();
    }, []);

    const location = useLocation();
    const [searchParams] = useSearchParams();
    const isProductsPath = location.pathname === "/products";
    const activeCategory = searchParams.get("categoryId");

    const [menuOpen, setMenuOpen] = useState(false);

    useEffect(() => {
        setMenuOpen(false);
    }, [location.pathname, location.search]);

    useEffect(() => {
        function onKeyDown(e) {
            if (e.key === "Escape") setMenuOpen(false);
        }
        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, []);

    useEffect(() => {
        function onResize() {
            if (window.innerWidth >= 860) setMenuOpen(false);
        }
        window.addEventListener("resize", onResize);
        return () => window.removeEventListener("resize", onResize);
    }, []);

    return (
        <nav className="nav" role="navigation" aria-label="Main">
            <button
                className="nav_toggle"
                type="button"
                aria-controls="mainmenu"
                aria-expanded={menuOpen ? "true" : "false"}
                aria-label="Toggle main menu"
                onClick={() => setMenuOpen(o => !o)}
            >
                <span className="nav_toggleBar" />
                <span className="nav_toggleBar" />
                <span className="nav_toggleBar" />
            </button>
            <ul id="mainmenu" className={"nav_list" + (menuOpen ? " is-open" : "")}>
                <li className="nav_item">
                    <NavLink
                        to="/products"
                        end
                        className={() => "nav_link" + (isProductsPath && !activeCategory ? " is-active" : "")}
                        onClick={() => setMenuOpen(false)}
                    >
                        All Products
                    </NavLink>
                </li>
                {categories.map(c => {
                    const to = `/products?categoryId=${encodeURIComponent(c.categoryId)}`;
                    const isActiveCat = isProductsPath && String(activeCategory) === String(c.categoryId);
                    return (
                        <li key={c.categoryId} className="nav_item">
                            <NavLink
                                to={to}
                                className={() => "nav_link" + (isActiveCat ? " is-active" : "")}
                                onClick={() => setMenuOpen(false)}
                            >
                                {c.name}
                            </NavLink>
                        </li>
                    );
                })}
            </ul>
        </nav>
    )
}