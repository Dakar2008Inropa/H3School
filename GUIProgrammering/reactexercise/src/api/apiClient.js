let rawBase = "https://dummyjson.com/products";
if (typeof import.meta !== "undefined" && import.meta.env && import.meta.env.VITE_API_URL) {
    rawBase = String(import.meta.env.VITE_API_URL);
}
const API_BASE = rawBase.replace(/\/+$/, "");

function buildUrl(relativePath, params) {
    const rel = relativePath ? String(relativePath).replace(/^\/+/, "") : "";
    const target = rel.length > 0 ? `${API_BASE}/${rel}` : API_BASE;
    const url = new URL(target);

    if (params) {
        for (const [key, value] of Object.entries(params)) {
            if (value !== undefined && value !== null && value !== "") {
                url.searchParams.set(key, String(value));
            }
        }
    }
    return url.toString();
}

async function handleResponse(response) {
    const contentType = response.headers.get("content-type") ?? "";
    const isJson = contentType.includes("application/json");
    const payload = isJson ? await response.json() : await response.text();

    if (response.ok) {
        return payload;
    }

    const message = isJson && payload && payload.message ? payload.message : response.statusText;
    const error = new Error(message || "Request failed");
    error.status = response.status;
    error.payload = payload;
    throw error;
}

export async function get(path = "", params) {
    const url = buildUrl(path, params);
    const response = await fetch(url, {
        method: "GET",
        headers: {
            "Accept": "application/json"
        }
    });
    return handleResponse(response);
}

export async function post(path = "", body) {
    const url = buildUrl(path);
    const response = await fetch(url, {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify(body)
    });
    return handleResponse(response);
}

export async function put(path = "", body) {
    const url = buildUrl(path);
    const response = await fetch(url, {
        method: "PUT",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify(body)
    });
    return handleResponse(response);
}

export async function del(path = "") {
    const url = buildUrl(path);
    const response = await fetch(url, {
        method: "DELETE",
        headers: {
            "Accept": "application/json"
        }
    });
    return handleResponse(response);
}