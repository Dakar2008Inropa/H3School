/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useCallback, useEffect, useMemo, useState, useContext } from "react";

const STORAGE_KEY = "cart:v1";

function readStorage() {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return [];
        const parsed = JSON.parse(raw);
        return Array.isArray(parsed) ? parsed : [];
    } catch {
        return [];
    }
}

function writeStorage(items) {
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
    } catch {
        // Ignore
    }
}

const CartContext = createContext(null);

export function CartProvider({ children }) {
    const [items, setItems] = useState(() => readStorage());

    useEffect(() => {
        writeStorage(items);
    }, [items]);

    const addItem = useCallback((product, qty = 1) => {
        if (!product || product.id == null) return;
        const amount = Number(qty) || 1;
        setItems(prev => {
            const idx = prev.findIndex(x => String(x.id) === String(product.id));
            if (idx >= 0) {
                const next = prev.slice();
                next[idx] = { ...next[idx], qty: next[idx].qty + amount };
                return next;
            }
            const { id, title, price, image } = product;
            return [...prev, { id, title, price: Number(price) || 0, image: image || "", qty: amount }];
        });
    }, []);

    const removeItem = useCallback((id) => {
        setItems(prev => prev.filter(x => String(x.id) !== String(id)));
    }, []);

    const setQty = useCallback((id, qty) => {
        const value = Math.max(1, Number(qty) || 1);
        setItems(prev => prev.map(x => String(x.id) === String(id) ? { ...x, qty: value } : x));
    }, []);

    const inc = useCallback((id) => {
        setItems(prev => prev.map(x => String(x.id) === String(id) ? { ...x, qty: x.qty + 1 } : x));
    }, []);

    const dec = useCallback((id) => {
        setItems(prev => prev.map(x => {
            if (String(x.id) !== String(id)) return x;
            const next = Math.max(1, x.qty - 1);
            return { ...x, qty: next };
        }));
    }, []);

    const clear = useCallback(() => {
        setItems([]);
    }, []);

    const totalQuantity = useMemo(() => {
        return items.reduce((sum, it) => sum + (Number(it.qty) || 0), 0);
    }, [items]);

    const totalInclVat = useMemo(() => {
        return items.reduce((sum, it) => sum + (Number(it.price) || 0) * (Number(it.qty) || 0), 0);
    }, [items]);

    const VAT_RATE = 0.25;
    const totalExclVat = useMemo(() => {
        return totalInclVat / (1 + VAT_RATE);
    }, [totalInclVat]);

    const vatAmount = useMemo(() => {
        return totalInclVat - totalExclVat;
    }, [totalInclVat, totalExclVat]);

    const value = useMemo(() => ({
        items,
        addItem,
        removeItem,
        setQty,
        inc,
        dec,
        clear,
        totalQuantity,
        totalInclVat,
        totalExclVat,
        vatAmount,
        VAT_RATE
    }), [items, addItem, removeItem, setQty, inc, dec, clear, totalQuantity, totalInclVat, totalExclVat, vatAmount]);

    return (
        <CartContext.Provider value={value}>
            {children}
        </CartContext.Provider>
    );
}

export function useCart() {
    const ctx = useContext(CartContext);
    if (!ctx) {
        throw new Error("useCart must be used within a CartProvider.");
    }
    return ctx;
}