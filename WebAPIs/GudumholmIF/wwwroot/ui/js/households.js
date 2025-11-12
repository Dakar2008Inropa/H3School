(function () {
    "use strict";

    const $tableBody = $("#householdsTable tbody");
    const $messages = $("#messages");

    const $formTitle = $("#formTitle");
    const $householdId = $("#householdId");
    const $street = $("#street");
    const $postalCode = $("#postalCode");
    const $city = $("#city");
    const $saveBtn = $("#saveBtn");

    $("#reloadHouseholds").on("click", loadHouseholds);
    $("#resetBtn").on("click", resetForm);
    $("#householdForm").on("submit", onSubmit);

    $("#createHousehold").on("click", onCreateHousehold);

    let access = {
        isAdmin: false,
        isModerator: false,
        isUserOnly: false,
        allowCreate: false,
        allowUpdate: false,
        allowMutate: false
    };

    // [NEW] Kaldt efter Auth.ensureAuth
    window.PageAuthReady = function () {
        const roles = (window.Auth && window.Auth.currentUser && Array.isArray(window.Auth.currentUser.roles))
            ? window.Auth.currentUser.roles : [];
        const isAdmin = roles.includes("Administrator");
        const isModerator = roles.includes("Moderator");
        const isUser = roles.includes("User");
        access.isAdmin = isAdmin;
        access.isModerator = !isAdmin && isModerator;
        access.isUserOnly = !isAdmin && !isModerator && isUser;
        access.allowCreate = isAdmin; // moderator må ikke oprette
        access.allowUpdate = isAdmin || isModerator;
        access.allowMutate = isAdmin || isModerator;

        applyRoleUI();
        loadHouseholds();
        if (window.Auth) window.Auth.updateNav();
    };

    // [NEW] Skjul/disable UI
    function applyRoleUI() {
        if (!access.allowCreate) {
            $("#createHousehold").addClass("d-none");
        }
        if (access.isUserOnly) {
            $saveBtn.prop("disabled", true).addClass("disabled");
        }
    }

    function onCreateHousehold() {
        resetForm();
        $street.trigger("focus");
    }

    function showMessage(text, isError = false) {
        $messages
            .text(text)
            .removeClass("error ok")
            .addClass(isError ? "error" : "ok");
        setTimeout(() => $messages.text("").removeClass("error ok"), 4000);
    }

    function api(url, method, body) {
        return $.ajax({
            url: url,
            method: method,
            data: body ? JSON.stringify(body) : undefined,
            contentType: "application/json; charset=utf-8",
            headers: { "Accept": "application/json" }
        });
    }

    function loadHouseholds() {
        $tableBody.empty().append(`<tr><td colspan="6">Loading...</td></tr>`);
        api("/api/Households", "GET")
            .done(renderHouseholds)
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function renderHouseholds(list) {
        $tableBody.empty();
        if (!Array.isArray(list) || list.length === 0) {
            $tableBody.append(`<tr><td colspan="6">No households found.</td></tr>`);
            return;
        }
        for (const h of list) {
            const tr = $(`
                <tr>
                    <td>${escapeHtml(h.street)}</td>
                    <td class="text-center">${escapeHtml(h.postalCode)}</td>
                    <td class="text-center">${escapeHtml(h.city ?? "")}</td>
                    <td class="text-center">${h.memberCount ?? 0}</td>
                    <td class="actions">
                        <div class="actions-grid">
                            <button class="edit btn btn-sm btn-outline-primary me-1" data-id="${h.id}">Edit</button>
                            <button class="delete btn btn-sm btn-outline-danger" data-id="${h.id}">Delete</button>
                        </div>
                    </td>
                </tr>
            `);
            tr.find("button.edit").on("click", () => startEdit(h.id));
            tr.find("button.delete").on("click", () => doDelete(h.id));

            if (access.isUserOnly) {
                tr.find("button.delete").addClass("d-none");
            }

            $tableBody.append(tr);
        }
    }

    function startEdit(id) {
        api(`/api/Households/${id}`, "GET")
            .done(h => {
                $householdId.val(h.id);
                $street.val(h.street);
                $postalCode.val(h.postalCode);
                $city.val(h.city || "");

                $formTitle.text(`Edit household #${h.id}`);
                $saveBtn.text("Update");

                if (!access.allowUpdate) {
                    $saveBtn.prop("disabled", true).addClass("disabled");
                }
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function doDelete(id) {
        if (!access.allowMutate) {
            showMessage("You are not allowed to modify.", true);
            return;
        }

        if (!confirm(`Delete household #${id}?`)) return;
        api(`/api/Households/${id}`, "DELETE")
            .done(() => {
                showMessage("Household deleted.");
                loadHouseholds();
                if ($householdId.val() === String(id)) resetForm();
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function onSubmit(e) {
        e.preventDefault();

        const id = $householdId.val().trim();

        if (access.isUserOnly) {
            showMessage("You are not allowed to save changes.", true);
            return;
        }
        if (!id && !access.allowCreate) {
            showMessage("You are not allowed to create items.", true);
            return;
        }
        if (id && !access.allowUpdate) {
            showMessage("You are not allowed to update items.", true);
            return;
        }

        const payload = {
            street: $street.val().trim(),
            city: ($city.val() || "").trim(),
            postalCode: $postalCode.val().trim()
        };

        if (!payload.street || !payload.postalCode) {
            showMessage("Please fill out Street and Postal code.", true);
            return;
        }

        if (id) {
            api(`/api/Households/${id}`, "PUT", payload)
                .done(() => {
                    showMessage("Household updated.");
                    loadHouseholds();
                    startEdit(Number(id));
                })
                .fail(xhr => showMessage(extractError(xhr), true));
        } else {
            api("/api/Households", "POST", payload)
                .done(() => {
                    showMessage("Household created.");
                    loadHouseholds();
                    resetForm();
                })
                .fail(xhr => showMessage(extractError(xhr), true));
        }
    }

    function resetForm() {
        $householdId.val("");
        $street.val("");
        $postalCode.val("");
        $city.val("");
        $formTitle.text("Create household");
        $saveBtn.text("Create");

        applyRoleUI();
    }

    function extractError(xhr) {
        try {
            if (!xhr) return "Unexpected error.";

            if (xhr.responseJSON) {
                const p = xhr.responseJSON;
                const parts = [];

                if (p.title) parts.push(p.title);
                if (p.detail) parts.push(p.detail);
                if (typeof p.message === "string" && p.message.trim()) parts.push(p.message.trim());
                if (typeof p.error === "string" && p.error.trim()) parts.push(p.error.trim());

                if (p.errors && typeof p.errors === "object") {
                    const details = Object.entries(p.errors)
                        .flatMap(([k, arr]) => Array.isArray(arr) ? arr.map(m => `${k}: ${m}`) : [`${k}: ${String(arr)}`]);
                    if (details.length) parts.push(details.join(" | "));
                }

                if (parts.length) return parts.join(" — ");
            }

            const raw = (xhr.responseText || "").toString().trim();
            if (raw) {
                try {
                    const p = JSON.parse(raw);
                    const parts = [];
                    if (p.title) parts.push(p.title);
                    if (p.detail) parts.push(p.detail);
                    if (typeof p.message === "string" && p.message.trim()) parts.push(p.message.trim());
                    if (typeof p.error === "string" && p.error.trim()) parts.push(p.error.trim());
                    if (p.errors && typeof p.errors === "object") {
                        const details = Object.entries(p.errors)
                            .flatMap(([k, arr]) => Array.isArray(arr) ? arr.map(m => `${k}: ${m}`) : [`${k}: ${String(arr)}`]);
                        if (details.length) parts.push(details.join(" | "));
                    }
                    if (parts.length) return parts.join(" — ");
                } catch {
                    return raw.length > 400 ? raw.slice(0, 400) : raw;
                }
            }

            const code = xhr.status != null ? String(xhr.status) : "0";
            const statusText = xhr.statusText ? ` ${xhr.statusText}` : "";
            return `HTTP ${code}${statusText}`;
        } catch {
            const code = xhr?.status != null ? String(xhr.status) : "0";
            const statusText = xhr?.statusText ? ` ${xhr.statusText}` : "";
            return `HTTP ${code}${statusText}`;
        }
    }

    function escapeHtml(s) {
        if (s === null || s === undefined) return "";
        return String(s)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll("\"", "&quot;")
            .replaceAll("'", "&#39;");
    }
})();