import { http } from "./apiClient";

function toQuery(params) {
    // eslint-disable-next-line no-unused-vars
    const entries = Object.entries(params || {}).filter(([_, v]) => v !== undefined && v !== null && v !== "");
    if (entries.length === 0) return "";
    const qs = new URLSearchParams();
    for (const [k, v] of entries) {
        if (typeof v === "boolean") {
            qs.set(k, v ? "true" : "false");
        } else {
            qs.set(k, String(v));
        }
    }
    return `?${qs.toString()}`;
}

export const CategoriesApi = {
    list: ({ includeProducts = false, q, signal } = {}) => http.get(`/api/Categories${toQuery({ includeProducts, q })}`, { signal }),
    get: (id, signal) => http.get(`/api/Categories/${id}`, { signal }),
    create: (data) => http.post("/api/Categories", data),
    update: (id, data) => http.put(`/api/Categories/${id}`, data),
    remove: (id) => http.del(`/api/Categories/${id}`),
    search: (term, signal) => http.get(`/api/Categories/search?term=${encodeURIComponent(term)}`, { signal }),
};