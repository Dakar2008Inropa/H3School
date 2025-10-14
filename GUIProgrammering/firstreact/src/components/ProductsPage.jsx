import { useEffect, useState } from "react";
import { ProductsApi } from "../productsApi";
import { CategoriesApi } from "../categoriesApi";

function Alert({ message, details }) {
    if (!message) return null;
    return (
        <div role="alert" style={{ background: "#3b1f1f", color: "#ffdcdc", padding: 10, borderRadius: 8, marginTop: 8 }}>
            <div style={{ fontWeight: 600 }}>{message}</div>
            {Array.isArray(details) && details.length > 0 && (
                <ul style={{ margin: "6px 0 0 18px" }}>
                    {details.map((d, i) => <li key={i}>{d}</li>)}
                </ul>
            )}
        </div>
    );
}

function formatCurrency(value) {
    if (value == null || value === "" || isNaN(value)) return "-";

    return new Intl.NumberFormat("da-DK", {
        style: "currency",
        currency: "DKK",
        minimumFractionDigits: 2,
    }).format(value);
}

export default function ProductsPage() {
    const [products, setProducts] = useState([]);
    const [query, setQuery] = useState("");

    const [listError, setListError] = useState({ message: "", details: [] });
    const [formError, setFormError] = useState({ message: "", details: [] });

    const [showCreateProduct, setShowCreateProduct] = useState(false);
    const [showCreateCategory, setShowCreateCategory] = useState(false);

    const [categories, setCategories] = useState([]);
    const [loadingCats, setLoadingCats] = useState(false);

    const [newCategory, setNewCategory] = useState({ name: "" });

    const [newProduct, setNewProduct] = useState({ name: "", price: "", categoryId: "" });

    const [saving, setSaving] = useState(false);

    async function loadProducts() {
        try {
            setListError({ message: "", details: [] });
            const ctrl = new AbortController();

            const raw = query
                ? await ProductsApi.search(query, undefined, ctrl.signal)
                : await ProductsApi.list(ctrl.signal);

            const data = Array.isArray(raw)
                ? raw.map(p => ({
                    id: p.id ?? p.productId ?? 0,
                    name: p.name ?? p.productName ?? "",
                    price: p.price ?? 0,
                    categoryId: p.categoryId ?? null,
                    categoryName: p.categoryName ?? "",
                }))
                : [];

            setProducts(data);
        } catch (e) {
            setListError({ message: e.message || "Kunne ikke hente produkter", details: e.details || [] });
        }
    }

    async function loadCategories() {
        setLoadingCats(true);
        try {
            const data = await CategoriesApi.list();
            setCategories(data || []);
        } finally {
            setLoadingCats(false);
        }
    }

    useEffect(() => { loadProducts(); }, [query]);
    useEffect(() => { loadCategories(); }, []);

    function openCreateProduct() {
        setFormError({ message: "", details: [] });
        setShowCreateCategory(false);
        setShowCreateProduct(true);
    }
    function openCreateCategory() {
        setFormError({ message: "", details: [] });
        setShowCreateProduct(false);
        setShowCreateCategory(true);
    }

    function cancelProduct() {
        setShowCreateProduct(false);
        setNewProduct({ name: "", price: "", categoryId: "" });
        setSaving(false);
        setFormError({ message: "", details: [] });
    }
    function cancelCategory() {
        setShowCreateCategory(false);
        setNewCategory({ name: "" });
        setFormError({ message: "", details: [] });
    }

    async function submitCategory(e) {
        e.preventDefault();
        setFormError({ message: "", details: [] });

        const name = newCategory.name.trim();
        if (!name) {
            setFormError({ message: "Kategorinavn er påkrævet.", details: [] });
            return;
        }

        try {
            setSaving(true);
            const created = await CategoriesApi.create({ name });
            await loadCategories();
            setNewProduct(p => ({ ...p, categoryId: created?.id != null ? String(created.id) : "" }));
            setShowCreateCategory(false);
            setShowCreateProduct(true);
        } catch (e) {
            setFormError({ message: e.message, details: e.details || [] });
        } finally {
            setSaving(false);
        }
    }

    async function submitProduct(e) {
        e.preventDefault();
        setFormError({ message: "", details: [] });

        const name = newProduct.name.trim();
        const priceNumber = Number(newProduct.price);

        if (!name) return setFormError({ message: "Produktnavn er påkrævet.", details: [] });
        if (Number.isNaN(priceNumber) || priceNumber < 0)
            return setFormError({ message: "Pris skal være et ikke-negativt tal.", details: [] });
        if (!newProduct.categoryId)
            return setFormError({ message: "Vælg en kategori.", details: [] });

        try {
            setSaving(true);
            await ProductsApi.create({
                name,
                price: priceNumber,
                categoryId: Number(newProduct.categoryId)                
            });
            cancelProduct();
            await loadProducts();
        } catch (e) {
            setFormError({ message: e.message, details: e.details || [] });
        } finally {
            setSaving(false);
        }
    }

    async function deleteProduct(id) {
        const product = products.find(p => p.id === id);
        const navn = product ? product.name : "dette produkt";

        if (!window.confirm(`Er du sikker på, at du vil slette ${navn}?`)) {
            return;
        }


        setListError({ message: "", details: [] });
        try {
            await ProductsApi.remove(id);
            loadProducts();
        } catch (e) {
            setListError({ message: e.message || "Sletning fejlede", details: e.details || [] });
        }
    }

    return (
        <div style={{ maxWidth: 920, margin: "0 auto", padding: "24px 16px" }}>
            <h1>Produkter</h1>

            <div style={{ display: "flex", gap: 8, marginBottom: 12 }}>
                <input
                    placeholder="Søg produkter…"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    style={{ flex: 1, padding: 8 }}
                />
                <button onClick={openCreateProduct}>Nyt produkt</button>
                <button onClick={openCreateCategory}>Ny kategori</button>
            </div>

            <Alert message={listError.message} details={listError.details} />

            {showCreateCategory && (
                <form onSubmit={submitCategory} style={formStyle}>
                    <h2>Opret kategori</h2>

                    <div style={gridStyle}>
                        <label>Navn</label>
                        <input
                            value={newCategory.name}
                            onChange={(e) => setNewCategory(c => ({ ...c, name: e.target.value }))}
                            required
                            style={fieldStyle}
                        />
                    </div>

                    <Alert message={formError.message} details={formError.details} />

                    <div style={{ display: "flex", gap: 8, marginTop: 12 }}>
                        <button type="submit" disabled={saving}>{saving ? "Gemmer…" : "Opret kategori"}</button>
                        <button type="button" onClick={cancelCategory} disabled={saving}>Annuller</button>
                    </div>
                </form>
            )}

            {showCreateProduct && (
                <form onSubmit={submitProduct} style={formStyle}>
                    <h2>Opret produkt</h2>
                    {loadingCats ? <p>Henter kategorier…</p> : null}

                    <div style={gridStyle}>
                        <label>Navn</label>
                        <input
                            value={newProduct.name}
                            onChange={(e) => setNewProduct(p => ({ ...p, name: e.target.value }))}
                            required
                            style={fieldStyle}
                        />

                        <label>Pris</label>
                        <input
                            type="number"
                            step="0.01"
                            value={newProduct.price}
                            onChange={(e) => setNewProduct(p => ({ ...p, price: e.target.value }))}
                            required
                            style={fieldStyle}
                        />

                        <label>Kategori</label>
                        <select
                            value={newProduct.categoryId === "" ? "" : String(newProduct.categoryId)}
                            onChange={(e) => {
                                const val = e.target.value;
                                setNewProduct(p => ({
                                    ...p,
                                    categoryId: val === "" ? null : parseInt(val, 10)
                                }));
                            }}
                            required
                            style={fieldStyle}
                        >
                            <option value="">Vælg kategori…</option>
                            {categories.map(c => (
                                <option key={c.categoryId} value={c.categoryId}>{c.name}</option>
                            ))}
                        </select>
                    </div>

                    <Alert message={formError.message} details={formError.details} />

                    <div style={{ display: "flex", gap: 8, marginTop: 12 }}>
                        <button type="submit" disabled={saving}>{saving ? "Gemmer…" : "Opret produkt"}</button>
                        <button type="button" onClick={cancelProduct} disabled={saving}>Annuller</button>
                    </div>
                </form>
            )}

            <table style={{ width: "100%", borderCollapse: "collapse" }}>
                <thead>
                    <tr>
                        <th style={thStyle}>Navn</th>
                        <th style={thStyleCenter}>Pris</th>
                        <th style={thStyleCenter}>Handling</th>
                    </tr>
                </thead>
                <tbody>
                    {products.map(p => (
                        <tr key={p.id}>
                            <td>{p.name}</td>
                            <td style={cellTextCenter}>{formatCurrency(p.price)}</td>
                            <td style={{ display: "flex", gap: 8, paddingBottom: "6px", paddingTop: "6px" }}>
                                <button className="deleteBtn" onClick={() => deleteProduct(p.id)}>Slet</button>
                            </td>
                        </tr>
                    ))}
                    {products.length === 0 && (
                        <tr><td colSpan={3} style={{ padding: 12, color: "#999" }}>Ingen resultater</td></tr>
                    )}
                </tbody>
            </table>
        </div>
    );
}

const formStyle = { padding: 12, border: "1px solid #ddd", borderRadius: 8, marginBottom: 16 };
const gridStyle = { display: "grid", gridTemplateColumns: "140px 1fr", gap: 8, alignItems: "center" };
const fieldStyle = { width: "100%", padding: 8, boxSizing: "border-box" };
const thStyle = { textAlign: "left", borderBottom: "1px solid #ddd" };
const thStyleCenter = { textAlign: "center", borderBottom: "1px solid #ddd" };

const cellTextCenter = { textAlign: "center" };