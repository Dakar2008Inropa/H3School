(function () {
    "use strict";

    const $tableBody = $("#sportsTable tbody");
    const $messages = $("#messages");

    const $formTitle = $("#formTitle");
    const $sportId = $("#sportId");
    const $name = $("#name");
    const $annualFeeAdult = $("#annualFeeAdult");
    const $annualFeeChild = $("#annualFeeChild");
    const $isActive = $("#isActive");
    const $saveBtn = $("#saveBtn");

    $("#reloadSports").on("click", loadSports);
    $("#resetBtn").on("click", resetForm);
    $("#sportForm").on("submit", onSubmit);

    $("#createSport").on("click", onCreateSport);

    let access = {
        isAdmin: false,
        isModerator: false,
        isUserOnly: false,
        allowCreate: false,
        allowUpdate: false,
        allowMutate: false
    };

    window.PageAuthReady = function () {
        const roles = (window.Auth && window.Auth.currentUser && Array.isArray(window.Auth.currentUser.roles))
            ? window.Auth.currentUser.roles : [];
        const isAdmin = roles.includes("Administrator");
        const isModerator = roles.includes("Moderator");
        const isUser = roles.includes("User");
        access.isAdmin = isAdmin;
        access.isModerator = !isAdmin && isModerator;
        access.isUserOnly = !isAdmin && !isModerator && isUser;
        access.allowCreate = isAdmin;
        access.allowUpdate = isAdmin || isModerator;
        access.allowMutate = isAdmin || isModerator;

        applyRoleUI();
        loadSports();
        if (window.Auth) window.Auth.updateNav();
    };

    function applyRoleUI() {
        if (!access.allowCreate) {
            $("#createSport").addClass("d-none");
        }
        if (access.isUserOnly) {
            $saveBtn.prop("disabled", true).addClass("disabled");
        }
    }

    function onCreateSport() {
        resetForm();
        $name.trigger("focus");
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

    function loadSports() {
        $tableBody.empty().append(`<tr><td colspan="5">Loading...</td></tr>`);
        api("/api/Sports", "GET")
            .done(renderSports)
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function renderSports(list) {
        $tableBody.empty();
        if (!Array.isArray(list) || list.length === 0) {
            $tableBody.append(`<tr><td colspan="5">No sports found.</td></tr>`);
            return;
        }
        for (const s of list) {
            const tr = $(`
                <tr>
                    <td>${escapeHtml(s.name)}</td>
                    <td class="text-center">${Number(s.annualFeeAdult).toFixed(2)}</td>
                    <td class="text-center">${Number(s.annualFeeChild).toFixed(2)}</td>
                    <td class="text-center">${s.isActive ? "True" : "False"}</td>
                    <td class="actions">
                        <div class="actions-grid">
                            <button class="edit btn btn-sm btn-outline-primary me-1" data-id="${s.id}">Edit</button>
                            <button class="deactivate btn btn-sm btn-outline-warning" data-id="${s.id}" ${s.isActive ? "" : "disabled"}>Deactivate</button>
                        </div>
                    </td>
                </tr>
            `);
            tr.find("button.edit").on("click", () => startEdit(s.id));
            tr.find("button.deactivate").on("click", () => deactivateSport(s.id));

            if (access.isUserOnly) {
                tr.find("button.deactivate").addClass("d-none");
            }

            $tableBody.append(tr);
        }
    }

    function startEdit(id) {
        api(`/api/Sports/${id}`, "GET")
            .done(s => {
                $sportId.val(s.id);
                $name.val(s.name);
                $annualFeeAdult.val(Number(s.annualFeeAdult).toFixed(2));
                $annualFeeChild.val(Number(s.annualFeeChild).toFixed(2));
                $isActive.val(String(Boolean(s.isActive)));

                $formTitle.text(`Edit sport #${s.id}`);
                $saveBtn.text("Update");

                if (!access.allowUpdate) {
                    $saveBtn.prop("disabled", true).addClass("disabled");
                }
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function deactivateSport(id) {
        if (!access.allowMutate) {
            showMessage("You are not allowed to modify.", true);
            return;
        }

        if (!confirm(`Deactivate sport #${id}?`)) return;
        api(`/api/Sports/${id}`, "DELETE")
            .done(() => {
                showMessage("Sport deactivated.");
                loadSports();
                if ($sportId.val() === String(id)) {
                    $isActive.val("false");
                }
                startEdit(Number(id));
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function onSubmit(e) {
        e.preventDefault();

        const id = $sportId.val().trim();

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
            name: $name.val().trim(),
            isActive: $isActive.val() === "true",
            annualFeeAdult: Number($annualFeeAdult.val()),
            annualFeeChild: Number($annualFeeChild.val())
        };

        if (!payload.name ||
            !Number.isFinite(payload.annualFeeAdult) || payload.annualFeeAdult < 0 ||
            !Number.isFinite(payload.annualFeeChild) || payload.annualFeeChild < 0) {
            showMessage("Please provide valid Adult/Child annual fees.", true);
            return;
        }

        if (id) {
            api(`/api/Sports/${id}`, "PUT", payload)
                .done(() => {
                    showMessage("Sport updated.");
                    loadSports();
                    startEdit(Number(id));
                })
                .fail(xhr => showMessage(extractError(xhr), true));
        } else {
            api("/api/Sports", "POST", payload)
                .done(() => {
                    showMessage("Sport created.");
                    loadSports();
                    resetForm();
                })
                .fail(xhr => showMessage(extractError(xhr), true));
        }
    }

    function resetForm() {
        $sportId.val("");
        $name.val("");
        $annualFeeAdult.val("");
        $annualFeeChild.val("");
        $isActive.val("true");
        $formTitle.text("Create sport");
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