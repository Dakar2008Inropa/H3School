import { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import { ProductsApi } from "../../api/productsApi";
import { useAppNavigation } from "../../routes/useAppNavigation";
import { useCart } from "../../cart/CartContext";
import "./Product.css";

function toInt(v) {
    if (v === null || v === undefined) return null;
    const n = Number(v);
    return Number.isFinite(n) ? n : null;
}

function mapProduct(p) {
    const id = p?.id ?? p?.productId ?? 0;
    const title = p?.name ?? p?.title ?? "Untitled";
    const price = p?.price ?? 0;
    const image =
        typeof p?.imageUrl === "string" && p.imageUrl.length > 0
            ? p.imageUrl
            : "";
    const description =
        typeof p?.description === "string" && p.description.trim().length > 0
            ? p.description
            : "";
    const categoryName =
        p?.categoryName ?? p?.category?.name ?? p?.category?.title ?? "—";
    return { id, title, price, image, description, categoryName };
}

export const Product = () => {
    const { id } = useParams();

    const { back } = useAppNavigation();
    const { addItem } = useCart();

    const [product, setProduct] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const formatDkk = useMemo(() => {
        return (price) =>
            `kr. ${new Intl.NumberFormat("da-DK", {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
            }).format(Number(price) || 0)}`;
    }, []);

    useEffect(() => {
        const productId = toInt(id);
        if (!productId) {
            setError("Invalid product id.");
            setLoading(false);
            return;
        }

        const ac = new AbortController();

        async function load() {
            try {
                setLoading(true);
                setError("");

                const data = await ProductsApi.get(productId, ac.signal);
                const mapped = mapProduct(data);
                setProduct(mapped);
            } catch (e) {
                if (e?.name === "AbortError") return;
                setError(e?.message || "Could not load product.");
            } finally {
                setLoading(false);
            }
        }

        load();
        return () => ac.abort();
    }, [id]);

    const imageSrc =
        product?.image && typeof product.image === "string" && product.image.length > 0
            ? product.image
            : 'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="800" height="1067"><rect width="100%25" height="100%25" fill="%23e9e7e1"/><text x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" fill="%23907e69" font-family="sans-serif" font-size="24">No image</text></svg>';

    if (loading) {
        return (
            <main className="product-page">
                <a
                    href="#"
                    className="back-link"
                    onClick={(e) => {
                        e.preventDefault();
                        back();
                    }}
                >
                    ← Go back
                </a>
                <section className="product-layout">
                    <div className="product-media skeleton" />
                    <div className="product-content">
                        <div className="skeleton title" />
                        <div className="skeleton price" />
                        <div className="skeleton button" />
                        <div className="skeleton paragraph" />
                        <div className="skeleton paragraph" />
                    </div>
                </section>
            </main>
        );
    }

    if (error) {
        return (
            <main className="product-page">
                <a
                    href="#"
                    className="back-link"
                    onClick={(e) => {
                        e.preventDefault();
                        back();
                    }}>
                    ← Go back
                </a>
                <p className="error-text">Could not load product. Please try again later.</p>
            </main>
        );
    }

    if (!product) {
        return (
            <main className="product-page">
                <a
                    href="#"
                    className="back-link"
                    onClick={(e) => {
                        e.preventDefault();
                        back();
                    }}
                >
                    ← Go back
                </a>
                <p>No product found.</p>
            </main>
        );
    }

    return (
        <main className="product-page">
            <a
                href="#"
                className="back-link"
                onClick={(e) => {
                    e.preventDefault();
                    back();
                }}>
                ← Go back
            </a>

            <section className="product-layout">
                <div className="product-media-single">
                    <img
                        src={imageSrc}
                        alt={product.title}
                        className="product-image-single"
                        loading="eager"
                    />
                </div>

                <div className="product-content">
                    <div className="product-meta">
                        <span className="product-category">{product.categoryName}</span>
                    </div>

                    <h1 className="product-title product-title-single">{product.title}</h1>

                    <p className="product-price product-price-single">{formatDkk(product.price)}</p>

                    <button
                        type="button"
                        className="btn-add-to-cart"
                        onClick={() => addItem(product, 1)}
                    >
                        Add to cart
                    </button>

                    <div className="product-description">
                        <h2 className="desc-title">Description</h2>
                        {product.description ? (
                            product.description
                                .split(/\n{2,}/g)
                                .map((p, i) => (
                                    <p key={i} className="desc-paragraph">
                                        {p.trim()}
                                    </p>
                                ))
                        ) : (
                            <p className="desc-paragraph">No description available.</p>
                        )}
                    </div>
                </div>
            </section>
        </main>
    );
};