const BASE_URL = import.meta.env.VITE_API_URL || "https://localhost:7032";

function buildHeaders(isFormData) {
    return isFormData
        ? { Accept: "application/json" }
        : { "Content-Type": "application/json", Accept: "application/json" };
}

function isJsonResponse(contentType) {
    return (contentType || "").includes("application/json");
}

async function send(path, { method = "GET", body, signal } = {}) {
    const isFormData = typeof FormData !== "undefined" && body instanceof FormData;

    try {
        return await fetch(BASE_URL + path, {
            method,
            headers: buildHeaders(isFormData),
            body: isFormData ? body : (body ? JSON.stringify(body) : undefined),
            signal,
        });
    } catch (networkError) {
        const err = new Error("Could not connect to the API. Ensure the server is running.");
        err.status = 0;
        err.details = [networkError.message];
        throw err;
    }
}

function flattenValidationErrors(errors) {
    if (!errors || typeof errors !== "object") return [];
    return Object.entries(errors).flatMap(([field, msgs]) =>
        Array.isArray(msgs) ? msgs.map(m => `${field}: ${m}`) : [`${field}: ${msgs}`]
    );
}

async function readJsonSafe(response) {
    try {
        return { problem: await response.json(), parseError: null };
    } catch (e) {
        return { problem: null, parseError: e };
    }
}

function buildProblemAddition(problem) {
    if (!problem || typeof problem !== "object") return { addition: "", details: [] };

    const parts = [];
    if (problem.title) parts.push(problem.title);
    if (problem.detail) parts.push(problem.detail);

    const details = flattenValidationErrors(problem.errors);
    if (details.length) parts.push(details.join(" | "));

    return {
        addition: parts.length ? parts.join(" — ") : "",
        details
    };
}

async function buildErrorAddition(response, contentType) {
    if (isJsonResponse(contentType)) {
        const { problem, parseError } = await readJsonSafe(response);
        if (parseError) {
            return {
                addition: "Failed to parse error response from server.",
                details: [parseError.message]
            };
        }
        return buildProblemAddition(problem);
    }

    const text = await response.text();
    return { addition: text ? text.slice(0, 400) : "", details: [] };
}

async function readError(response, contentType) {
    const base = `HTTP ${response.status} ${response.statusText}`;

    const { addition, details } = await buildErrorAddition(response, contentType);

    const message = addition ? `${base} — ${addition}` : base;

    const err = new Error(message);
    err.status = response.status;
    err.details = details;
    throw err;
}

async function parseSuccess(response, contentType) {
    if (!isJsonResponse(contentType)) return undefined;

    try {
        return await response.json();
    } catch (jsonError) {
        const err = new Error("Invalid JSON response from server.");
        err.status = response.status;
        err.details = [jsonError.message];
        throw err;
    }
}

async function request(path, { method = "GET", body, signal } = {}) {
    const response = await send(path, { method, body, signal });
    const contentType = response.headers.get("content-type") || "";

    if (response.ok) return await parseSuccess(response, contentType);

    await readError(response, contentType);
    return undefined;
}

async function requestWithHeaders(path, { method = "GET", body, signal } = {}) {
    const response = await send(path, { method, body, signal });
    const contentType = response.headers.get("content-type") || "";

    if (response.ok) {
        const data = await parseSuccess(response, contentType);
        return { data, headers: response.headers };
    }

    await readError(response, contentType);
    return { data: undefined, headers: response.headers };
}

export const http = {
    get: (path, opts) => request(path, { ...opts, method: "GET" }),
    post: (path, body, opts) => request(path, { ...opts, method: "POST", body }),
    put: (path, body, opts) => request(path, { ...opts, method: "PUT", body }),
    del: (path, opts) => request(path, { ...opts, method: "DELETE" }),
    postForm: (path, formData, opts) => request(path, { ...opts, method: "POST", body: formData }),

    getWithHeaders: (path, opts) => requestWithHeaders(path, { ...opts, method: "GET" }),
    postWithHeaders: (path, body, opts) => requestWithHeaders(path, { ...opts, method: "POST", body }),
    putWithHeaders: (path, body, opts) => requestWithHeaders(path, { ...opts, method: "PUT", body }),
    delWithHeaders: (path, opts) => requestWithHeaders(path, { ...opts, method: "DELETE" }),
};