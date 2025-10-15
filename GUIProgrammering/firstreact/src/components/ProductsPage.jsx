import { useEffect, useState } from "react";
import { ProductsApi, ImageFilesApi } from "../productsApi";
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

    const [newProduct, setNewProduct] = useState({ name: "", price: "", categoryId: "", imageFileId: null });

    const [saving, setSaving] = useState(false);

    const [images, setImages] = useState([]);

    const [editId, setEditId] = useState(null);
    const [editModel, setEditModel] = useState(null);

    const [previewUrl, setPreviewUrl] = useState("");

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
                    imageFileId: p.imageFileId ?? null,
                    imageUrl: p.imageUrl ?? ""
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

    async function loadImages() {
        try {
            const ctrl = new AbortController();
            const data = await ImageFilesApi.list(ctrl.signal);
            setImages(Array.isArray(data) ? data : []);
        } catch (e) {
            console.warn("Failed to load images.", e);
        }
    }

    useEffect(() => { loadProducts(); }, [query]);
    useEffect(() => { loadCategories(); loadImages(); }, []);

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
        setNewProduct({ name: "", price: "", categoryId: "", imageFileId: null });
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
                categoryId: Number(newProduct.categoryId),
                imageFileId: newProduct.imageFileId == null ? null : Number(newProduct.imageFileId)
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

    async function handleCreateUpload(files) {
        if (!files || files.length === 0) return;
        try {
            const uploaded = await ImageFilesApi.upload(files);
            if (Array.isArray(uploaded) && uploaded.length > 0) {
                const first = uploaded[0];
                setImages((prev) => {
                    const exists = prev.some(x => x.imageFileId === first.imageFileId);
                    return exists ? prev : [...prev, first];
                });
                setNewProduct(p => ({ ...p, imageFileId: first.imageFileId }));
            }
        } catch (e) {
            setFormError({ message: e.message || "Upload fejlede.", details: e.details || [] });
        }
    }

    function startEdit(row) {
        setEditId(row.id);
        setEditModel({
            id: row.id,
            name: row.name,
            price: row.price,
            categoryId: row.categoryId,
            imageFileId: row.imageFileId ?? null
        });
        setFormError({ message: "", details: [] });
    }

    function cancelEdit() {
        setEditId(null);
        setEditModel(null);
    }

    async function saveEdit() {
        if (!editModel) return;
        try {
            setSaving(true);
            const body = {
                productId: Number(editModel.id),
                name: String(editModel.name || "").trim(),
                price: Number(editModel.price),
                categoryId: Number(editModel.categoryId),
                imageFileId: editModel.imageFileId == null ? null : Number(editModel.imageFileId)
            };
            await ProductsApi.update(editModel.id, body);

            setProducts(prev => prev.map(p => {
                if (p.id !== editModel.id) return p;
                const image = body.imageFileId ? images.find(x => x.imageFileId === body.imageFileId) : null;
                return {
                    ...p,
                    name: body.name,
                    price: body.price,
                    categoryId: body.categoryId,
                    imageFileId: body.imageFileId,
                    imageUrl: image?.url || (body.imageFileId ? p.imageUrl : "")
                };
            }));

            cancelEdit();
        } catch (e) {
            setFormError({ message: e.message || "Opdatering fejlede.", details: e.details || [] });
        } finally {
            setSaving(false);
        }
    }

    async function handleEditUpload(files) {
        if (!files || files.length === 0) return;
        try {
            const uploaded = await ImageFilesApi.upload(files);
            if (Array.isArray(uploaded) && uploaded.length > 0) {
                const first = uploaded[0];
                setImages((prev) => {
                    const exists = prev.some(x => x.imageFileId === first.imageFileId);
                    return exists ? prev : [...prev, first];
                });
                setEditModel(m => ({ ...m, imageFileId: first.imageFileId }));
            }
        } catch (e) {
            setFormError({ message: e.message || "Upload fejlede.", details: e.details || [] });
        }
    }

    function showImage(url) {
        if (!url) {
            setListError({ message: "Dette produkt har ikke noget billede.", details: [] });
            return;
        }
        setPreviewUrl(url);
    }
    function closePreview() {
        setPreviewUrl("");
    }

    return (
        <div style={{ maxWidth: 980, margin: "0 auto", padding: "24px 16px" }}>
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

                        { }
                        <label>Billede</label>
                        <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
                            <select
                                value={newProduct.imageFileId == null ? "" : String(newProduct.imageFileId)}
                                onChange={(e) => {
                                    const val = e.target.value;
                                    setNewProduct(p => ({ ...p, imageFileId: val === "" ? null : parseInt(val, 10) }));
                                }}
                                style={{ ...fieldStyle }}
                            >
                                <option value="">(intet)</option>
                                {images.map(img => (
                                    <option key={img.imageFileId} value={img.imageFileId}>{img.fileName}</option>
                                ))}
                            </select>
                            { }
                            <input
                                type="file"
                                accept=".jpg,.jpeg,.png,.gif,.bmp,.webp,.svg"
                                onChange={(e) => handleCreateUpload(e.target.files)}
                            />
                        </div>
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
                        { }
                        <th style={thStyleCenter}>Billede</th>
                        <th style={thStyleCenter}>Handling</th>
                    </tr>
                </thead>
                <tbody>
                    {products.map(p => {
                        const isEditing = editId === p.id;

                        if (isEditing && editModel) {
                            return (
                                <tr key={p.id}>
                                    <td>
                                        <input
                                            value={editModel.name}
                                            onChange={(e) => setEditModel(m => ({ ...m, name: e.target.value }))}
                                            style={fieldStyle}
                                        />
                                    </td>
                                    <td style={cellTextCenter}>
                                        <input
                                            type="number"
                                            step="0.01"
                                            value={editModel.price}
                                            onChange={(e) => setEditModel(m => ({ ...m, price: e.target.value }))}
                                            style={{ ...fieldStyle, maxWidth: 140 }}
                                        />
                                    </td>
                                    <td style={cellTextCenter}>
                                        { }
                                        <div style={{ display: "flex", gap: 8, justifyContent: "center" }}>
                                            <select
                                                value={editModel.imageFileId == null ? "" : String(editModel.imageFileId)}
                                                onChange={(e) => {
                                                    const val = e.target.value;
                                                    setEditModel(m => ({ ...m, imageFileId: val === "" ? null : parseInt(val, 10) }));
                                                }}
                                            >
                                                <option value="">(intet)</option>
                                                {images.map(img => (
                                                    <option key={img.imageFileId} value={img.imageFileId}>{img.fileName}</option>
                                                ))}
                                            </select>
                                            <input
                                                type="file"
                                                accept=".jpg,.jpeg,.png,.gif,.bmp,.webp,.svg"
                                                onChange={(e) => handleEditUpload(e.target.files)}
                                            />
                                        </div>
                                    </td>
                                    <td style={{ display: "flex", gap: 8, paddingBottom: "6px", paddingTop: "6px", justifyContent: "center" }}>
                                        <button onClick={saveEdit} disabled={saving}>{saving ? "Gemmer…" : "Gem"}</button>
                                        <button onClick={cancelEdit} disabled={saving}>Annuller</button>
                                    </td>
                                </tr>
                            );
                        }

                        return (
                            <tr key={p.id}>
                                <td>{p.name}</td>
                                <td style={cellTextCenter}>{formatCurrency(p.price)}</td>
                                <td style={{ ...cellTextCenter, paddingTop: 6, paddingBottom: 6 }}>
                                    {p.imageUrl
                                        ? <img src={p.imageUrl} alt={p.name} style={{ width: 48, height: 48, objectFit: "cover", borderRadius: 4 }} />
                                        : <span style={{ color: "#999" }}>(intet)</span>
                                    }
                                </td>
                                <td style={{ display: "flex", gap: 8, paddingBottom: "6px", paddingTop: "6px", justifyContent: "center" }}>
                                    { }
                                    <button onClick={() => startEdit(p)}>Rediger</button>
                                    <button className="deleteBtn" onClick={() => deleteProduct(p.id)}>Slet</button>
                                    <button onClick={() => showImage(p.imageUrl)}>Vis Billede</button>
                                </td>
                            </tr>
                        );
                    })}
                    {products.length === 0 && (
                        <tr><td colSpan={4} style={{ padding: 12, color: "#999" }}>Ingen resultater</td></tr>
                    )}
                </tbody>
            </table>

            { }
            {previewUrl ? (
                <div
                    onClick={closePreview}
                    style={modalBackdropStyle}
                >
                    <div style={modalContentStyle} onClick={(e) => e.stopPropagation()}>
                        <div style={{ marginBottom: 8, textAlign: "right" }}>
                            <button onClick={closePreview}>Luk</button>
                        </div>
                        <img src={previewUrl} alt="Preview" style={{ maxWidth: "80vw", maxHeight: "80vh" }} />
                    </div>
                </div>
            ) : null}
        </div>
    );
}

const formStyle = { padding: 12, border: "1px solid #ddd", borderRadius: 8, marginBottom: 16 };
const gridStyle = { display: "grid", gridTemplateColumns: "140px 1fr", gap: 8, alignItems: "center" };
const fieldStyle = { width: "100%", padding: 8, boxSizing: "border-box" };
const thStyle = { textAlign: "left", borderBottom: "1px solid #ddd" };
const thStyleCenter = { textAlign: "center", borderBottom: "1px solid #ddd" };

const cellTextCenter = { textAlign: "center" };

const modalBackdropStyle = {
    position: "fixed",
    inset: 0,
    background: "rgba(0,0,0,0.6)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    zIndex: 1000
};
const modalContentStyle = {
    background: "#fff",
    padding: 16,
    borderRadius: 6,
    boxShadow: "0 6px 24px rgba(0,0,0,0.4)"
};