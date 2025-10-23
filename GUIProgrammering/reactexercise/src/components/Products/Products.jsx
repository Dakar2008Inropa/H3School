import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { fetchProducts } from "../../api/productsApi.js";
import { GridContainer } from "../GridContainer/GridContainer";
import { InfoCard } from "../InfoCard/InfoCard";

export const Products = ({ infoCardHeight = "480px" }) => {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [limit, setLimit] = useState(9);
    const [search, setSearch] = useState("");
    const [sortBy, setSortBy] = useState("title");
    const [sortOrder, setSortOrder] = useState("asc");

    const navigate = useNavigate();

    const allowedLimits = useMemo(() => [9, 18, 36, 72, 144, 288], []);

    const fmt = useMemo(() => new Intl.NumberFormat("da-DK"), []);

    useEffect(() => {
        const controller = new AbortController();

        async function load() {
            setLoading(true);
            setError(null);

            try {
                const data = await fetchProducts({
                    limit
                });

                if (controller.signal.aborted) return;

                const normalized = Array.isArray(data)
                    ? data.map(p => ({
                        id: p.id,
                        title: p.title,
                        description: p.description,
                        price: p.price,
                        imageSrc: (Array.isArray(p.images) && p.images.length > 0)
                            ? p.images[0]
                            : (p.thumbnail ?? "https://placehold.co/600x400?text=No+image"),
                        imageAlt: `${p.title} image`
                    }))
                    : [];

                setProducts(normalized);
            } catch (e) {
                if (controller.signal.aborted) return;

                console.error("[Products] Failed to load products", {
                    error: e,
                    status: e?.status,
                    payload: e?.payload
                });
                setError("Failed to load products. Please try again.");
            } finally {
                if (!controller.signal.aborted) {
                    setLoading(false);
                }
            }
        }

        load();
        return () => controller.abort();
    }, [limit]);

    const view = useMemo(() => {
        let data = products;

        const q = search.trim().toLowerCase();
        if (q.length > 0) {
            data = data.filter(p => p.title?.toLowerCase().includes(q));
        }

        const sorted = [...data].sort((a, b) => {
            if (sortBy === "price") {
                const av = Number(a.price) || 0;
                const bv = Number(b.price) || 0;
                return av - bv;
            }
            const at = a.title ?? "";
            const bt = b.title ?? "";
            return at.localeCompare(bt, "da", { sensitivity: "base" });
        });

        if (sortOrder === "desc") sorted.reverse();
        return sorted;
    }, [products, search, sortBy, sortOrder]);

    if (loading) {
        return (
            <div style={{ padding: 12, color: "#ddd" }}>Loading products...</div>
        );
    }

    if (error) {
        return (
            <div style={{ padding: 12, color: "#ddd" }}>{error}</div>
        );
    }

    return (
        <div
            style={{
                width: "100%",
                ["--info-card-height"]: infoCardHeight
            }}
        >
            <div
                style={{
                    width: "100%",
                    display: "flex",
                    flexWrap: "wrap",
                    gap: 12,
                    justifyContent: "center"
                }}
            >
                <label
                    htmlFor="product-search"
                    style={{
                        display: "flex",
                        alignItems: "center",
                        gap: 8,
                        padding: "8px 12px",
                        background: "var(--panel-bg, #0f172a)",
                        border: "1px solid var(--panel-border, rgba(255,255,255,0.12))",
                        borderRadius: 8,
                        boxShadow: "0 1px 2px rgba(0,0,0,0.04)"
                    }}
                >
                    <span style={{ fontSize: 14, color: "var(--muted-fg, #e5e7eb)" }}>Search title:</span>
                    <input
                        id="product-search"
                        placeholder="Type to search…"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        style={{
                            padding: "8px 12px",
                            minWidth: 220,
                            borderRadius: 6,
                            border: "1px solid var(--panel-border, rgba(255,255,255,0.15))",
                            backgroundColor: "var(--control-bg, #111827)",
                            color: "var(--control-fg, #ffffff)",
                            outline: "none",
                            boxShadow: "0 0 0 2px transparent"
                        }}
                    />
                </label>
                <label
                    htmlFor="product-limit"
                    style={{
                        display: "flex",
                        alignItems: "center",
                        gap: 8,
                        padding: "8px 12px",
                        background: "var(--panel-bg, #0f172a)",
                        border: "1px solid var(--panel-border, rgba(255,255,255,0.12))",
                        borderRadius: 8,
                        boxShadow: "0 1px 2px rgba(0,0,0,0.04)"
                    }}
                >
                    <span style={{ fontSize: 14, color: "var(--muted-fg, #e5e7eb)" }}>Products to display:</span>
                    <select
                        id="product-limit"
                        value={limit}
                        onChange={(e) => {
                            const value = Number.parseInt(e.target.value, 10);
                            setLimit(Number.isNaN(value) ? 9 : value);
                        }}
                        style={{
                            padding: "8px 12px",
                            minWidth: 140,
                            borderRadius: 6,
                            border: "1px solid var(--panel-border, rgba(255,255,255,0.15))",
                            backgroundColor: "var(--control-bg, #111827)",
                            color: "var(--control-fg, #ffffff)",
                            outline: "none",
                            boxShadow: "0 0 0 2px transparent",
                            cursor: "pointer",
                            appearance: "none",
                            WebkitAppearance: "none",
                            MozAppearance: "none"
                        }}
                    >
                        {allowedLimits.map(v => (
                            <option key={v} value={v}>{v}</option>
                        ))}
                    </select>
                </label>
                <label
                    htmlFor="product-sortby"
                    style={{
                        display: "flex",
                        alignItems: "center",
                        gap: 8,
                        padding: "8px 12px",
                        background: "var(--panel-bg, #0f172a)",
                        border: "1px solid var(--panel-border, rgba(255,255,255,0.12))",
                        borderRadius: 8,
                        boxShadow: "0 1px 2px rgba(0,0,0,0.04)"
                    }}
                >
                    <span style={{ fontSize: 14, color: "var(--muted-fg, #e5e7eb)" }}>Sort by:</span>
                    <select
                        id="product-sortby"
                        value={sortBy}
                        onChange={(e) => setSortBy(e.target.value)}
                        style={{
                            padding: "8px 12px",
                            minWidth: 140,
                            borderRadius: 6,
                            border: "1px solid var(--panel-border, rgba(255,255,255,0.15))",
                            backgroundColor: "var(--control-bg, #111827)",
                            color: "var(--control-fg, #ffffff)",
                            outline: "none",
                            boxShadow: "0 0 0 2px transparent",
                            cursor: "pointer",
                            appearance: "none"
                        }}
                    >
                        <option value="title">Title</option>
                        <option value="price">Price</option>
                    </select>
                </label>
                <label
                    htmlFor="product-order"
                    style={{
                        display: "flex",
                        alignItems: "center",
                        gap: 8,
                        padding: "8px 12px",
                        background: "var(--panel-bg, #0f172a)",
                        border: "1px solid var(--panel-border, rgba(255,255,255,0.12))",
                        borderRadius: 8,
                        boxShadow: "0 1px 2px rgba(0,0,0,0.04)"
                    }}
                >
                    <span style={{ fontSize: 14, color: "var(--muted-fg, #e5e7eb)" }}>Order:</span>
                    <select
                        id="product-order"
                        value={sortOrder}
                        onChange={(e) => setSortOrder(e.target.value)}
                        style={{
                            padding: "8px 12px",
                            minWidth: 140,
                            borderRadius: 6,
                            border: "1px solid var(--panel-border, rgba(255,255,255,0.15))",
                            backgroundColor: "var(--control-bg, #111827)",
                            color: "var(--control-fg, #ffffff)",
                            outline: "none",
                            boxShadow: "0 0 0 2px transparent",
                            cursor: "pointer",
                            appearance: "none"
                        }}
                    >
                        <option value="asc">Ascending</option>
                        <option value="desc">Descending</option>
                    </select>
                </label>
            </div>
            <GridContainer>
                {view.map(p => {
                    const priceText = `${fmt.format(Number(p.price) || 0)} kr`;
                    return (
                        <InfoCard
                            key={p.id}
                            productId={p.id}
                            title={p.title}
                            imageSrc={p.imageSrc}
                            imageAlt={p.imageAlt}
                            price={priceText}
                            description={p.description}
                        />
                    );
                })}
            </GridContainer>
        </div>
    );
};