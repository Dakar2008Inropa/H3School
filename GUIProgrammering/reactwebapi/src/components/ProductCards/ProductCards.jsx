import "../FeatureProducts/FeatureProducts.css";
import "./ProductCards.css";

function formatDkk(price) {
  const value = Number(price) || 0;
  const formatted = new Intl.NumberFormat("da-DK", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
  return `kr. ${formatted}`;
}

function ProductCard({ product }) {
  const src =
    product.image && typeof product.image === "string" && product.image.length > 0
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

export const ProductCards = ({ items = [], loading = false, error = "" }) => {
  if (loading) {
    return (
      <section className="feature-products">
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
        <p className="error-text">Could not get products. Please try again later.</p>
      </section>
    );
  }

  if (!items.length) {
    return (
      <section className="feature-products">
        <p>No products found.</p>
      </section>
    );
  }

  return (
    <section className="feature-products">
      <div className="feature-products-grid">
        {items.map((p) => (
          <ProductCard key={p.id} product={p} />
        ))}
      </div>
    </section>
  );
};