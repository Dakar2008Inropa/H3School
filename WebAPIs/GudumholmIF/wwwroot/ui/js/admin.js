(function () {
    "use strict";

    const $err = $("#adminError");
    const $rolesList = $("#rolesList");
    const $formCreateRole = $("#formCreateRole");
    const $roleName = $("#roleName");
    const $usersTable = $("#usersTable");

    const $apiKeyValue = $("#apiKeyValue");
    const $btnCopyApiKey = $("#btnCopyApiKey");
    const $btnRotateApiKey = $("#btnRotateApiKey");

    function showError(msg) {
        $err.text(msg || "Unexpected error.").removeClass("d-none");
    }
    function clearError() { $err.addClass("d-none").empty(); }

    async function api(url, method, body) {
        const res = await fetch(url, {
            method: method || "GET",
            headers: body ? { "Content-Type": "application/json", "Accept": "application/json" } : { "Accept": "application/json" },
            credentials: "include",
            body: body ? JSON.stringify(body) : undefined
        });
        if (!res.ok) {
            let msg = "HTTP " + res.status;
            try {
                const p = await res.json();
                msg = p?.message || msg;
            } catch { }
            throw new Error(msg);
        }
        if (res.status === 204) return null;
        return await res.json().catch(() => null);
    }

    async function loadApiKey() {
        try {
            clearError();
            const data = await api("/api/Settings/api-key", "GET");
            $apiKeyValue.val(data?.apiKey || "");
        } catch (e) {
            showError("Failed to load API key: " + e.message);
        }
    }

    async function rotateApiKey() {
        try {
            clearError();
            const data = await api("/api/Settings/api-key/rotate", "POST", {});
            $apiKeyValue.val(data?.apiKey || "");
        } catch (e) {
            showError("Failed to rotate API key: " + e.message);
        }
    }

    async function copyApiKey() {
        try {
            clearError();
            const value = String($apiKeyValue.val() || "");
            if (!value) {
                showError("No API key to copy.");
                return;
            }
            await navigator.clipboard.writeText(value);
        } catch (e) {
            showError("Copy failed.");
        }
    }

    async function loadRoles() {
        try {
            const roles = await api("/api/auth/roles", "GET");
            $rolesList.empty();
            if (!Array.isArray(roles) || roles.length === 0) {
                $rolesList.append(`<li class="list-group-item bg-dark text-secondary">No roles.</li>`);
                return;
            }
            for (const r of roles) {
                $rolesList.append(`<li class="list-group-item bg-dark">${escapeHtml(r.name)}</li>`);
            }
        } catch (e) {
            showError(e.message);
        }
    }

    async function createRole(name) {
        if (!name) return;
        await api("/api/auth/roles", "POST", { name: name });
    }

    async function loadUsers() {
        try {
            const users = await api("/api/auth/users", "GET");
            $usersTable.empty();
            if (!Array.isArray(users) || users.length === 0) {
                $usersTable.append(`<tr><td colspan="5" class="text-secondary">No users.</td></tr>`);
                return;
            }
            for (const u of users) {
                const id = u.id;
                const roles = Array.isArray(u.roles) ? u.roles : [];
                const rolesStr = roles.join(", ");

                const twoFaEnabled = Boolean(u.twoFactorEnabled === true);
                const twoFaPending = Boolean(u.twoFactorSetupPending === true);
                const twoFaText = twoFaEnabled ? "Active" : (twoFaPending ? "Pending" : "Disabled");
                const twoFaBadgeClass = twoFaEnabled
                    ? "bg-success"
                    : (twoFaPending ? "bg-warning text-dark" : "bg-secondary");

                const tr = $(`
                    <tr>
                        <td>${escapeHtml(u.userName || "")}</td>
                        <td>${escapeHtml(u.email || "")}</td>
                        <td>
                            <input class="form-control form-control-sm" id="roles-${id}" value="${escapeHtml(rolesStr)}" placeholder="Comma separated roles" />
                        </td>
                        <td>
                            <!-- [EDITED] Vis tre-tilstands badge -->
                            <span class="badge ${twoFaBadgeClass}">${twoFaText}</span>
                        </td>
                        <td class="text-end">
                            <!-- [EDITED] Erstat btn-group med flex + gap for mere luft -->
                            <div class="d-flex justify-content-end gap-2">
                                <button class="btn btn-sm btn-outline-primary" data-action="save-roles" data-id="${id}">Save roles</button>
                                <button class="btn btn-sm btn-outline-warning" data-action="init-2fa" data-id="${id}">Initiate 2FA</button>
                                <button class="btn btn-sm btn-outline-danger" data-action="disable-2fa" data-id="${id}">Disable 2FA</button>
                            </div>
                        </td>
                    </tr>
                `);

                tr.find("[data-action='save-roles']").on("click", async function () {
                    try {
                        clearError();
                        const raw = String($(`#roles-${id}`).val() || "");
                        const list = raw.split(",").map(s => s.trim()).filter(Boolean);
                        await api(`/api/auth/users/${id}/roles`, "PUT", { roles: list });
                        await loadUsers();
                    } catch (e) { showError(e.message); }
                });

                tr.find("[data-action='init-2fa']").on("click", async function () {
                    try {
                        clearError();
                        await api(`/api/auth/users/${id}/2fa/initiate`, "POST", {});
                        await loadUsers();
                    } catch (e) { showError(e.message); }
                });

                tr.find("[data-action='disable-2fa']").on("click", async function () {
                    try {
                        clearError();
                        await api(`/api/auth/users/${id}/2fa/disable`, "POST", {});
                        await loadUsers();
                    } catch (e) { showError(e.message); }
                });

                $usersTable.append(tr);
            }
        } catch (e) {
            showError(e.message);
        }
    }

    $formCreateRole.on("submit", async function (e) {
        e.preventDefault(); clearError();
        const name = String($roleName.val() || "").trim();
        if (!name) {
            showError("Enter a role name.");
            return;
        }
        try {
            await createRole(name);
            $roleName.val("");
            await loadRoles();
        } catch (e) {
            showError(e.message);
        }
    });

    $btnCopyApiKey.on("click", async function () { await copyApiKey(); });
    $btnRotateApiKey.on("click", async function () { await rotateApiKey(); });

    function escapeHtml(s) {
        if (s === null || s === undefined) return "";
        return String(s)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll("\"", "&quot;")
            .replaceAll("'", "&#39;");
    }

    $(async function () {
        await loadApiKey();
        await loadRoles();
        await loadUsers();
        if (window.Auth) window.Auth.updateNav();
    });
})();