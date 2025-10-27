const BASE_URL = import.meta.env.VITE_API_URL || "https://localhost:7032";

async function request(path, { method = "GET", body, signal } = {}) {
    const isFormData = typeof FormData !== "undefined" && body instanceof FormData;

    let response;
    try {
        response = await fetch(BASE_URL + path, {
            method,
            headers: isFormData
                ? { Accept: "application/json" }
                : { "Content-Type": "application/json", Accept: "application/json" },
            body: isFormData ? body : (body ? JSON.stringify(body) : undefined),
            signal,
        });
    } catch (networkError) {
        const err = new Error("Could not connect to the API. Ensure the server is running.");
        err.status = 0;
        err.details = [networkError.message];
        throw err;
    }

    const contentType = response.headers.get("content-type") || "";

    if (!response.ok) {
        let message = `HTTP ${response.status} ${response.statusText}`;
        let details = [];

        try {
            if (contentType.includes("application/json")) {
                const problem = await response.json();

                if (problem) {
                    const parts = [];
                    if (problem.title) parts.push(problem.title);
                    if (problem.detail) parts.push(problem.detail);

                    if (problem.errors && typeof problem.errors === "object") {
                        details = Object.entries(problem.errors).flatMap(([field, msgs]) =>
                            Array.isArray(msgs)
                                ? msgs.map(m => `${field}: ${m}`)
                                : [`${field}: ${msgs}`]
                        );
                        if (details.length) parts.push(details.join(" | "));
                    }

                    if (parts.length) message = parts.join(" — ");
                }
            } else {
                const text = await response.text();
                if (text) message += ` — ${text.slice(0, 400)}`;
            }
        } catch (parseError) {
            message += " — Failed to parse error response from server.";
            details.push(parseError.message);
        }

        const err = new Error(message);
        err.status = response.status;
        err.details = details;
        throw err;
    }

    if (contentType.includes("application/json")) {
        try {
            return await response.json();
        } catch (jsonError) {
            const err = new Error("Invalid JSON response from server.");
            err.status = response.status;
            err.details = [jsonError.message];
            throw err;
        }
    }

    return undefined;
}

export const http = {
    get: (path, opts) => request(path, { ...opts, method: "GET" }),
    post: (path, body, opts) => request(path, { ...opts, method: "POST", body }),
    put: (path, body, opts) => request(path, { ...opts, method: "PUT", body }),
    del: (path, opts) => request(path, { ...opts, method: "DELETE" }),
    postForm: (path, formData, opts) => request(path, { ...opts, method: "POST", body: formData }),
};