import { get, post, put, del } from "./apiClient.js";

export async function fetchProducts(options = {}) {
    const limit = typeof options.limit === "number" ? options.limit : undefined;
    const hasSearch = typeof options.search === "string" && options.search.trim().length > 0;

    if (hasSearch) {
        const result = await get("search", { q: options.search, limit });
        if (Array.isArray(result?.products)) {
            return result.products;
        }
        return [];
    }

    const result = await get("", { limit });
    if (Array.isArray(result?.products)) {
        return result.products;
    }
    return [];
}

export async function fetchProductById(id) {
    const isValidNumber = typeof id === "number" && Number.isFinite(id);
    const isValidString = typeof id === "string" && id.trim().length > 0;

    if (isValidNumber || isValidString) {
        return get(String(id));
    }

    throw new Error("Product id is required.");
}

export async function createProduct(product) {
    return post("add", product);
}

export async function updateProduct(id, product) {
    const isValidNumber = typeof id === "number" && Number.isFinite(id);
    const isValidString = typeof id === "string" && id.trim().length > 0;

    if (isValidNumber || isValidString) {
        return put(String(id), product);
    }

    throw new Error("Product id is required.");
}

export async function deleteProduct(id) {
    const isValidNumber = typeof id === "number" && Number.isFinite(id);
    const isValidString = typeof id === "string" && id.trim().length > 0;

    if (isValidNumber || isValidString) {
        return del(String(id));
    }

    throw new Error("Product id is required.");
}