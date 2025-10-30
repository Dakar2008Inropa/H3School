import { useMemo } from "react";
import { Link } from "react-router-dom";
import { useCart } from "../../cart/CartContext";
import "./Cart.css";

function formatDkk(n) {
    const v = Number(n) || 0;
    return `kr. ${new Intl.NumberFormat("da-DK", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v)}`;
}

export const Cart = () => {
    const { items, inc, dec, setQty, removeItem, clear, totalQuantity, totalInclVat, totalExclVat, vatAmount } = useCart();

    const hasItems = items.length > 0;

    const lines = useMemo(() => {
        return items.map(it => ({
            ...it,
            lineTotal: (Number(it.price) || 0) * (Number(it.qty) || 0)
        }));
    }, [items]);

    return (
        <main className="cart-page">
            <h1 className="cart-title">Shopping cart</h1>

            {!hasItems && (
                <div className="cart-empty">
                    <p>Your cart is empty.</p>
                    <Link to="/products" className="cart-link">Browse products</Link>
                </div>
            )}

            {hasItems && (
                <div className="cart-layout">
                    <section className="cart-items">
                        <table className="cart-table">
                            <thead>
                                <tr>
                                    <th>Product</th>
                                    <th className="col-price">Price</th>
                                    <th className="col-qty">Qty</th>
                                    <th className="col-total">Total</th>
                                    <th className="col-actions">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {lines.map(it => (
                                    <tr key={it.id}>
                                        <td className="cell-product">
                                            <Link to={`/products/${it.id}`} className="product-link" aria-label={`View ${it.title}`}>
                                                <div className="product-cell">
                                                    <img
                                                        src={it.image || 'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="160" height="120"><rect width="100%25" height="100%25" fill="%23e9e7e1"/></svg>'}
                                                        alt={it.title}
                                                        className="thumb"
                                                        loading="lazy"
                                                    />
                                                    <div className="meta">
                                                        <div className="name">{it.title}</div>
                                                    </div>
                                                </div>
                                            </Link>
                                        </td>
                                        <td className="cell-price">{formatDkk(it.price)}</td>
                                        <td className="cell-qty">
                                            <div className="qty">
                                                <button type="button" className="qtyBtn" onClick={() => dec(it.id)} aria-label="Decrease quantity">−</button>
                                                <input
                                                    className="qtyInput"
                                                    value={it.qty}
                                                    inputMode="numeric"
                                                    onChange={(e) => setQty(it.id, e.target.value)}
                                                    aria-label="Quantity"
                                                />
                                                <button type="button" className="qtyBtn" onClick={() => inc(it.id)} aria-label="Increase quantity">+</button>
                                            </div>
                                        </td>
                                        <td className="cell-total">{formatDkk(it.lineTotal)}</td>
                                        <td className="cell-actions">
                                            <button type="button" className="link danger" onClick={() => removeItem(it.id)}>Remove</button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>

                        <div className="cart-actions">
                            <Link to="/products" className="link">Continue shopping</Link>
                            <button type="button" className="btn-clear" onClick={clear}>Clear cart</button>
                        </div>
                    </section>

                    <aside className="cart-summary" aria-label="Order summary">
                        <h2 className="summary-title">Summary</h2>
                        <div className="row">
                            <span>Items</span>
                            <strong>{totalQuantity}</strong>
                        </div>
                        <div className="row">
                            <span>Total (excl. VAT)</span>
                            <strong>{formatDkk(totalExclVat)}</strong>
                        </div>
                        <div className="row">
                            <span>VAT</span>
                            <strong>{formatDkk(vatAmount)}</strong>
                        </div>
                        <div className="row total">
                            <span>Total (incl. VAT)</span>
                            <strong>{formatDkk(totalInclVat)}</strong>
                        </div>
                        <button type="button" className="btn-checkout" disabled title="Demo only">Checkout</button>
                    </aside>
                </div>
            )}
        </main>
    );
};