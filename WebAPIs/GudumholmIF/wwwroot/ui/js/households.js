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
                    <td>${h.id}</td>
                    <td>${escapeHtml(h.street)}</td>
                    <td>${escapeHtml(h.postalCode)}</td>
                    <td>${escapeHtml(h.city ?? "")}</td>
                    <td>${h.memberCount ?? 0}</td>
                    <td class="actions">
                        <button class="edit btn btn-sm btn-outline-primary me-1" data-id="${h.id}">Edit</button>
                        <button class="delete btn btn-sm btn-outline-danger" data-id="${h.id}">Delete</button>
                    </td>
                </tr>
            `);
            tr.find("button.edit").on("click", () => startEdit(h.id));
            tr.find("button.delete").on("click", () => doDelete(h.id));
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
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function doDelete(id) {
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
                    resetForm();
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
    }

    function extractError(xhr) {
        try {
            if (xhr && xhr.responseJSON) {
                const problem = xhr.responseJSON;
                const parts = [];
                if (problem.title) parts.push(problem.title);
                if (problem.detail) parts.push(problem.detail);
                if (problem.errors) {
                    const details = Object.entries(problem.errors)
                        .flatMap(([k, arr]) => Array.isArray(arr) ? arr.map(m => `${k}: ${m}`) : [`${k}: ${String(arr)}`]);
                    if (details.length) parts.push(details.join(" | "));
                }
                return parts.length ? parts.join(" — ") : `HTTP ${xhr.status} ${xhr.statusText}`;
            }
        } catch { }
        return xhr?.responseText || `HTTP ${xhr?.status || 0}`;
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

    loadHouseholds();
})();