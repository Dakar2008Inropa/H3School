import { http } from "./apiClient";

export const CategoriesApi = {
  list: (signal) => http.get("/api/Categories", { signal }),
  get: (id, signal) => http.get(`/api/Categories/${id}`, { signal }),
  create: (data) => http.post("/api/Categories", data),
  update: (id, data) => http.put(`/api/Categories/${id}`, data),
  remove: (id) => http.del(`/api/Categories/${id}`),
  search: (term, signal) => http.get(`/api/Categories/search?term=${encodeURIComponent(term)}`, { signal }),
};
