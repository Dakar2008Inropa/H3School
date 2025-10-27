import React, { useEffect, useState } from 'react';
import { ProductsApi } from '../../api/productsApi';
import './FeatureProducts.css';

function formatDkk(price) {
    const value = Number(price) || 0;
    const formatted = new Intl.NumberFormat('da-DK', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
    return `kr. ${formatted}`;
}

function mapProduct(p) {
    const id = p?.id ?? p?.productId ?? 0;
    const title = p?.name ?? p?.title ?? 'Untitled';
    const price = p?.price ?? 0;
    const image =
        typeof p?.imageUrl === 'string' ? p.imageUrl : '';
    return { id, title, price, image };
}

function ProductCard({ product }) {
    const src = product.image && typeof product.image === 'string' && product.image.length > 0
        ? product.image
        : 'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="800" height="600"><rect width="100%25" height="100%25" fill="%23e9dfd2"/><text x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" fill="%23907e69" font-family="sans-serif" font-size="24">No image</text></svg>';

    return (
        <article className="product-card">
            <div className="product-media">
                <img src={src} alt={product.title} loading="lazy" />
            </div>
            <div className="product-body">
                <h3 className="product-title">{product.title}</h3>
                <p className="product-price">{formatDkk(product.price)}</p>
            </div>
        </article>
    );
}

export const FeatureProducts = () => {
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const ac = new AbortController();

        async function load() {
            try {
                setLoading(true);
                setError('');

                const raw = await ProductsApi.list(ac.signal);
                const list = Array.isArray(raw) ? raw.map(mapProduct) : [];

                const three = list.slice().sort(() => Math.random() - 0.5).slice(0, 3);

                setItems(three.length > 0 ? three : list.slice(0, 3));
            } catch (e) {
                if (e?.name === 'AbortError') return;
                setError(e?.message || 'Failed to load products.');
            } finally {
                setLoading(false);
            }
        }

        load();
        return () => ac.abort();
    }, []);

    if (loading) {
        return (
            <section className="feature-products">
                <h1 className="section-title">Featured products</h1>
                <div className="feature-products-grid">
                    <div className="product-card skeleton" />
                    <div className="product-card skeleton" />
                    <div className="product-card skeleton" />
                </div>
            </section>
        );
    }

    if (error) {
        return (
            <section className="feature-products">
                <h1 className="section-title">Featured products</h1>
                <p className="error-text">Could not load products. Please try again later.</p>
            </section>
        );
    }

    return (
        <section className="feature-products">
            <h1 className="section-title">Featured products</h1>
            <div className="feature-products-grid">
                {items.map(p => (
                    <ProductCard key={p.id} product={p} />
                ))}
            </div>
        </section>
    );
};