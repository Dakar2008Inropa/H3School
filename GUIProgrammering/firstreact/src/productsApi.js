import { http } from "./apiClient";

function qs(params) {
    const sp = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
        if (v !== undefined && v !== null && v !== "") sp.append(k, v);
    });
    return `?${sp.toString()}`;
}

export const ProductsApi = {
    list: (signal) => http.get("/api/Products", { signal }),

    search: (q, categoryId, signal) =>
        http.get(`/api/Products/search${qs({ q, categoryId })}`, { signal }),

    get: (id, signal) => http.get(`/api/Products/${id}`, { signal }),
    create: (data) => http.post("/api/Products", data),
    update: (id, data) => http.put(`/api/Products/${id}`, data),
    remove: (id) => http.del(`/api/Products/${id}`),
};
