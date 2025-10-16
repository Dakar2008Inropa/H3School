(function () {
    "use strict";

    const $tableBody = $("#sportsTable tbody");
    const $messages = $("#messages");

    const $formTitle = $("#formTitle");
    const $sportId = $("#sportId");
    const $name = $("#name");
    const $annualFee = $("#annualFee");
    const $isActive = $("#isActive");
    const $saveBtn = $("#saveBtn");

    $("#reloadSports").on("click", loadSports);
    $("#resetBtn").on("click", resetForm);
    $("#sportForm").on("submit", onSubmit);

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
                    <td>${s.id}</td>
                    <td>${escapeHtml(s.name)}</td>
                    <td>${Number(s.annualFee).toFixed(2)}</td>
                    <td>${s.isActive ? "True" : "False"}</td>
                    <td class="actions">
                        <button class="edit btn btn-sm btn-outline-primary me-1" data-id="${s.id}">Edit</button>
                        <button class="deactivate btn btn-sm btn-outline-warning" data-id="${s.id}" ${s.isActive ? "" : "disabled"}>Deactivate</button>
                    </td>
                </tr>
            `);
            tr.find("button.edit").on("click", () => startEdit(s.id));
            tr.find("button.deactivate").on("click", () => deactivateSport(s.id));
            $tableBody.append(tr);
        }
    }

    function startEdit(id) {
        api(`/api/Sports/${id}`, "GET")
            .done(s => {
                $sportId.val(s.id);
                $name.val(s.name);
                $annualFee.val(Number(s.annualFee).toFixed(2));
                $isActive.val(String(Boolean(s.isActive)));

                $formTitle.text(`Edit sport #${s.id}`);
                $saveBtn.text("Update");
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function deactivateSport(id) {
        if (!confirm(`Deactivate sport #${id}?`)) return;
        api(`/api/Sports/${id}`, "DELETE")
            .done(() => {
                showMessage("Sport deactivated.");
                loadSports();
                if ($sportId.val() === String(id)) {
                    $isActive.val("false");
                }
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function onSubmit(e) {
        e.preventDefault();

        const id = $sportId.val().trim();
        const payload = {
            name: $name.val().trim(),
            isActive: $isActive.val() === "true",
            annualFee: Number($annualFee.val())
        };

        if (!payload.name || !Number.isFinite(payload.annualFee) || payload.annualFee < 0) {
            showMessage("Please provide a valid Name and Annual fee.", true);
            return;
        }

        if (id) {
            api(`/api/Sports/${id}`, "PUT", payload)
                .done(() => {
                    showMessage("Sport updated.");
                    loadSports();
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
        $annualFee.val("");
        $isActive.val("true");
        $formTitle.text("Create sport");
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

    loadSports();
})();