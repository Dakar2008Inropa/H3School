import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { fetchProductById } from "../../api/productsApi.js";
import "./ShowProduct.css";

export const ShowProduct = () => {
    const { id } = useParams();
    const navigate = useNavigate();

    const [product, setProduct] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [index, setIndex] = useState(0);

    const fmt = useMemo(() => new Intl.NumberFormat("da-DK"), []);

    useEffect(() => {
        let isActive = true;

        async function load() {
            setLoading(true);
            setError(null);
            try {
                const data = await fetchProductById(id);
                if (!isActive) return;
                setProduct(data || null);
                setIndex(0);
            } catch (e) {
                if (!isActive) return;
                console.error("[ShowProduct] Failed to load product", {
                    error: e,
                    status: e?.status,
                    payload: e?.payload
                });
                setError("Failed to load product. Please try again.");
            } finally {
                if (isActive) setLoading(false);
            }
        }

        load();
        return () => { isActive = false; };
    }, [id]);

    const images = useMemo(() => {
        if (!product) return [];
        if (Array.isArray(product.images) && product.images.length > 0) return product.images;
        if (product.thumbnail) return [product.thumbnail];
        return ["https://placehold.co/800x500?text=No+image"];
    }, [product]);

    function prev() {
        if (images.length <= 1) return;
        setIndex((i) => (i - 1 + images.length) % images.length);
    }
    function next() {
        if (images.length <= 1) return;
        setIndex((i) => (i + 1) % images.length);
    }

    if (loading) {
        return <div className="showprod__loading">Loading product...</div>;
    }

    if (error) {
        return <div className="showprod__error">{error}</div>;
    }

    if (!product) {
        return <div className="showprod__error">No product found.</div>;
    }

    const priceText = `${fmt.format(Number(product.price) || 0)} kr`;

    return (
        <section className="showprod__wrap">
            <div className="showprod__toolbar">
                <button type="button" className="showprod__back" onClick={() => navigate(-1)}>
                    Back
                </button>
            </div>

            <div className="showprod__grid">
                <div className="showprod__gallery">
                    <div className="showprod__carousel">
                        <img
                            src={images[index]}
                            alt={`${product.title} image ${index + 1}`}
                            className="showprod__image"
                            loading="lazy"
                        />
                        {images.length > 1 && (
                            <>
                                <button type="button" className="showprod__nav showprod__nav--prev" onClick={prev} aria-label="Previous image">‹</button>
                                <button type="button" className="showprod__nav showprod__nav--next" onClick={next} aria-label="Next image">›</button>
                            </>
                        )}
                    </div>
                    {images.length > 1 && (
                        <div className="showprod__thumbs">
                            {images.map((src, i) => (
                                <button
                                    key={src + i}
                                    type="button"
                                    className={"showprod__thumb" + (i === index ? " is-active" : "")}
                                    onClick={() => setIndex(i)}
                                    aria-label={`Show image ${i + 1}`}
                                >
                                    <img src={src} alt={`${product.title} thumbnail ${i + 1}`} loading="lazy" />
                                </button>
                            ))}
                        </div>
                    )}
                </div>
                <div className="showprod__details">
                    <h2 className="showprod__title">{product.title}</h2>
                    <div className="showprod__price">{priceText}</div>
                    <p className="showprod__desc">{product.description}</p>

                    <div className="showprod__meta">
                        <div><span className="showprod__label">Rating:</span> {product.rating ?? "-"}</div>
                        <div><span className="showprod__label">Brand:</span> {product.brand ?? "-"}</div>
                        <div><span className="showprod__label">SKU:</span> {product.sku ?? "-"}</div>
                        <div><span className="showprod__label">Weight:</span> {product.weight != null ? `${product.weight} g` : "-"}</div>
                        <div><span className="showprod__label">Warranty:</span> {product.warrantyInformation ?? "-"}</div>
                        <div><span className="showprod__label">Shipping:</span> {product.shippingInformation ?? "-"}</div>
                    </div>
                </div>
            </div>
        </section>
    );
};