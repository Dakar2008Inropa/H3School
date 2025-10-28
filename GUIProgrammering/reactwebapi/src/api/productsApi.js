import { http } from "./apiClient";

function qs(params) {
  const sp = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== "") sp.append(k, v);
  });
  return `?${sp.toString()}`;
}

function parsePaging(headers) {
  const get = (n) =>
    headers?.get?.(n) ??
    headers?.[n] ??
    headers?.[n?.toLowerCase?.()] ??
    undefined;

  const toInt = (v) => (v == null ? undefined : Number.parseInt(v, 10));

  return {
    pageNumber: toInt(get("X-Page-Number")),
    pageSize: toInt(get("X-Page-Size")),
    totalCount: toInt(get("X-Total-Count")),
    totalPages: toInt(get("X-Total-Pages")),
  };
}

async function getPaged(url, params = {}, signal) {
  const query = qs(params);
  const { data, headers } = await http.getWithHeaders(`${url}${query}`, {
    signal,
  });
  return { items: data, paging: parsePaging(headers) };
}

export const ProductsApi = {
  list: (signal) => http.get("/api/Products", { signal }),

  listPaged: (opts = {}, signal) => {
    const { q, categoryId, pageNumber = 1, pageSize = 10 } = opts;
    return getPaged(
      "/api/Products",
      { q, categoryId, pageNumber, pageSize },
      signal
    );
  },

  search: (q, categoryId, signal) =>
    getPaged("/api/Products", { q, categoryId }, signal),

  get: (id, signal) => http.get(`/api/Products/${id}`, { signal }),

  create: (data) => http.post("/api/Products", data),

  update: (id, data) => http.put(`/api/Products/${id}`, data),

  remove: (id) => http.del(`/api/Products/${id}`),
};

export const ImageFilesApi = {
  list: (signal) => http.get("/api/ImageFiles", { signal }),

  upload: (files, signal) => {
    const form = new FormData();
    for (const f of files) {
      form.append("files", f);
    }
    return http.postForm("/api/ImageFiles/upload", form, { signal });
  },

  refresh: (signal) => http.post("/api/ImageFiles/refresh", {}, { signal }),
};
