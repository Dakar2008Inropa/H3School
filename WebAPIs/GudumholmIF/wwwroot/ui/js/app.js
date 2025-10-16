(function () {
    "use strict";

    const $tableBody = $("#personsTable tbody");
    const $messages = $("#messages");

    const $formTitle = $("#formTitle");
    const $personId = $("#personId");
    const $cpr = $("#cpr");
    const $firstName = $("#firstName");
    const $lastName = $("#lastName");
    const $dob = $("#dob");
    const $householdSelect = $("#householdSelect");
    const $membershipState = $("#membershipState");
    const $saveBtn = $("#saveBtn");

    const $parentStatus = $("#parentStatus");
    const $makeParentBtn = $("#makeParentBtn");

    const $boardSportSelect = $("#boardSportSelect");
    const $boardFrom = $("#boardFrom");
    const $assignBoardRoleBtn = $("#assignBoardRoleBtn");
    const $boardRolesTBody = $("#boardRolesTable tbody");

    const $joinSportSelect = $("#joinSportSelect");
    const $joinSportBtn = $("#joinSportBtn");
    const $personSportsTBody = $("#personSportsTable tbody");

    let sportsCache = [];
    let sportsById = new Map();
    let currentPerson = null;

    $("#reloadPersons").on("click", loadPersons);
    $("#resetBtn").on("click", resetForm);
    $("#personForm").on("submit", onSubmit);

    $makeParentBtn.on("click", onMakeParent);
    $assignBoardRoleBtn.on("click", onAssignBoardRole);
    $joinSportBtn.on("click", onJoinSport);

    function applyCprMask(raw) {
        const digitsOnly = String(raw || "").replace(/\D/g, "").slice(0, 10);
        if (digitsOnly.length <= 6) return digitsOnly;
        return digitsOnly.slice(0, 6) + "-" + digitsOnly.slice(6);
    }

    $cpr.on("input", function () {
        const masked = applyCprMask($cpr.val());
        if ($cpr.val() !== masked) $cpr.val(masked);
    });

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
            method: method || "GET",
            data: body ? JSON.stringify(body) : undefined,
            contentType: body ? "application/json; charset=utf-8" : undefined,
            headers: { "Accept": "application/json" }
        });
    }

    function loadHouseholds() {
        $householdSelect.empty().append(`<option value="">Loading…</option>`);
        return api("/api/Households", "GET")
            .done(list => {
                $householdSelect.empty().append(`<option value="">Select household…</option>`);
                if (Array.isArray(list)) {
                    for (const h of list) {
                        const label = `${escapeHtml(h.street)}, ${escapeHtml(h.postalCode)}`;
                        const opt = $(`<option></option>`)
                            .attr("value", h.id)
                            .text(label);
                        $householdSelect.append(opt);
                    }
                }
            })
            .fail(xhr => {
                $householdSelect.empty().append(`<option value="">Failed to load households</option>`);
                showMessage(extractError(xhr), true);
            });
    }

    function loadSports() {
        $boardSportSelect.empty().append(`<option value="">Loading…</option>`);
        $joinSportSelect.empty().append(`<option value="">Loading…</option>`);
        return api("/api/Sports", "GET")
            .done(list => {
                sportsCache = Array.isArray(list) ? list : [];
                sportsById = new Map(sportsCache.map(s => [s.id, s]));
                const active = sportsCache.filter(s => s.isActive);
                const fillSelect = (sel) => {
                    sel.empty().append(`<option value="">Select sport…</option>`);
                    for (const s of active) {
                        sel.append($("<option></option>").attr("value", s.id).text(s.name));
                    }
                };
                fillSelect($boardSportSelect);
                fillSelect($joinSportSelect);
            })
            .fail(xhr => {
                $boardSportSelect.empty().append(`<option value="">Failed to load</option>`);
                $joinSportSelect.empty().append(`<option value="">Failed to load</option>`);
                showMessage(extractError(xhr), true);
            });
    }

    function loadPersons() {
        $tableBody.empty().append(`<tr><td colspan="9">Loading...</td></tr>`);
        api("/api/Persons", "GET")
            .done(renderPersons)
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function renderPersons(list) {
        $tableBody.empty();
        if (!Array.isArray(list) || list.length === 0) {
            $tableBody.append(`<tr><td colspan="9">No persons found.</td></tr>`);
            return;
        }
        for (const p of list) {
            const tr = $(`
                <tr>
                    <td>${p.id}</td>
                    <td>${escapeHtml(p.cpr)}</td>
                    <td>${escapeHtml(p.firstName)}</td>
                    <td>${escapeHtml(p.lastName)}</td>
                    <td>${escapeHtml(p.dateOfBirth)}</td>
                    <td>${p.householdId}</td>
                    <td>${escapeHtml(p.membershipState)}</td>
                    <td>${p.activeChildrenCount ?? 0}</td>
                    <td class="actions">
                        <!-- [EDITED] Bootstrap-styled action buttons -->
                        <button class="edit btn btn-sm btn-outline-primary me-1" data-id="${p.id}">Edit</button>
                        <button class="delete btn btn-sm btn-outline-danger" data-id="${p.id}">Delete</button>
                    </td>
                </tr>
            `);
            tr.find("button.edit").on("click", () => startEdit(p.id));
            tr.find("button.delete").on("click", () => doDelete(p.id));
            $tableBody.append(tr);
        }
    }

    function startEdit(id) {
        api(`/api/Persons/${id}`, "GET")
            .done(p => {
                currentPerson = p;
                $personId.val(p.id);
                $cpr.val(applyCprMask(p.cpr));
                $firstName.val(p.firstName);
                $lastName.val(p.lastName);
                $dob.val(p.dateOfBirth);
                $householdSelect.val(String(p.householdId));
                $membershipState.val(p.membershipState === "Active" ? "Active" : "Passive");

                $formTitle.text(`Edit person #${p.id}`);
                $saveBtn.text("Update");

                updateParentUI(p);

                loadBoardRoles(p.id);

                loadPersonSports(p.id);
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function updateParentUI(p) {
        const status = p.hasParentRole ? `Yes (Active children: ${p.activeChildrenCount ?? 0})` : "No";
        $parentStatus.text(status);
        $makeParentBtn.prop("disabled", !!p.hasParentRole || !p.id);
    }

    function onMakeParent() {
        const id = $personId.val().trim();
        if (!id) {
            showMessage("Select a person first.", true);
            return;
        }
        api(`/api/Persons/${id}/parent`, "POST", {})
            .done(updated => {
                currentPerson = updated;
                showMessage("Parent role created.");
                updateParentUI(updated);
                loadPersons();
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function loadBoardRoles(personId) {
        $boardRolesTBody.empty().append(`<tr><td colspan="5">Loading...</td></tr>`);
        api(`/api/BoardRoles/by-person/${personId}`, "GET")
            .done(list => {
                $boardRolesTBody.empty();
                if (!Array.isArray(list) || list.length === 0) {
                    $boardRolesTBody.append(`<tr><td colspan="5">No board roles.</td></tr>`);
                    return;
                }
                for (const r of list) {
                    const sport = sportsById.get(r.sportId);
                    const tr = $(`
                        <tr>
                            <td>${r.id}</td>
                            <td>${escapeHtml(sport ? sport.name : String(r.sportId))}</td>
                            <td>${escapeHtml(r.from)}</td>
                            <td>${r.to || ""}</td>
                            <td>
                                <button class="btn btn-sm btn-outline-warning close-role" data-id="${r.id}" ${r.to ? "disabled" : ""}>Close</button>
                            </td>
                        </tr>
                    `);
                    tr.find(".close-role").on("click", () => closeBoardRole(r.id));
                    $boardRolesTBody.append(tr);
                }
            })
            .fail(xhr => {
                $boardRolesTBody.empty().append(`<tr><td colspan="5">Failed to load roles.</td></tr>`);
                showMessage(extractError(xhr), true);
            });
    }

    function onAssignBoardRole(e) {
        e.preventDefault();
        const personId = $personId.val().trim();
        const sportId = Number($boardSportSelect.val());
        const from = $boardFrom.val() || new Date().toISOString().slice(0, 10);

        if (!personId || !sportId) {
            showMessage("Select a person and a sport.", true);
            return;
        }

        const payload = { personId: Number(personId), sportId: sportId, from: from };
        api("/api/BoardRoles", "POST", payload)
            .done(() => {
                showMessage("Board role assigned.");
                loadBoardRoles(Number(personId));
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function closeBoardRole(roleId) {
        const to = new Date().toISOString().slice(0, 10);
        api(`/api/BoardRoles/${roleId}/close`, "POST", { to: to })
            .done(() => {
                showMessage("Board role closed.");
                if (currentPerson?.id) loadBoardRoles(currentPerson.id);
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function loadPersonSports(personId) {
        $personSportsTBody.empty().append(`<tr><td colspan="4">Loading...</td></tr>`);
        api(`/api/persons/${personId}/sports`, "GET")
            .done(list => {
                $personSportsTBody.empty();
                if (!Array.isArray(list) || list.length === 0) {
                    $personSportsTBody.append(`<tr><td colspan="4">No sports memberships.</td></tr>`);
                    return;
                }
                for (const ps of list) {
                    const sport = sportsById.get(ps.sportId);
                    const tr = $(`
                        <tr>
                            <td>${escapeHtml(sport ? sport.name : String(ps.sportId))}</td>
                            <td>${escapeHtml(ps.joined)}</td>
                            <td>${ps.left || ""}</td>
                            <td>
                                <button class="btn btn-sm btn-outline-danger leave-sport" data-sport="${ps.sportId}" ${ps.left ? "disabled" : ""}>Leave</button>
                            </td>
                        </tr>
                    `);
                    tr.find(".leave-sport").on("click", () => leaveSport(personId, ps.sportId));
                    $personSportsTBody.append(tr);
                }
            })
            .fail(xhr => {
                $personSportsTBody.empty().append(`<tr><td colspan="4">Failed to load memberships.</td></tr>`);
                showMessage(extractError(xhr), true);
            });
    }

    function onJoinSport(e) {
        e.preventDefault();
        const personId = $personId.val().trim();
        const sportId = Number($joinSportSelect.val());
        if (!personId || !sportId) {
            showMessage("Select a person and a sport.", true);
            return;
        }
        api(`/api/persons/${personId}/sports/join`, "POST", { sportId: sportId })
            .done(() => {
                showMessage("Joined sport.");
                loadPersonSports(Number(personId));
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function leaveSport(personId, sportId) {
        const left = new Date().toISOString().slice(0, 10);
        api(`/api/persons/${personId}/sports/${sportId}/leave`, "POST", { left: left })
            .done(() => {
                showMessage("Left sport.");
                loadPersonSports(Number(personId));
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function doDelete(id) {
        if (!confirm(`Delete person #${id}?`)) return;
        api(`/api/Persons/${id}`, "DELETE")
            .done(() => {
                showMessage("Person deleted.");
                loadPersons();
                if ($personId.val() === String(id)) {
                    resetForm();
                }
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function onSubmit(e) {
        e.preventDefault();

        const id = $personId.val().trim();
        const householdIdValue = $householdSelect.val();
        if (!householdIdValue) {
            showMessage("Please select a household.", true);
            return;
        }

        const cprFormatted = applyCprMask($cpr.val());
        $cpr.val(cprFormatted);
        if (!/^\d{6}-\d{4}$/.test(cprFormatted)) {
            showMessage("CPR must be 10 digits in the format DDMMYY-XXXX.", true);
            return;
        }

        const payload = {
            cPR: cprFormatted,
            firstName: $firstName.val().trim(),
            lastName: $lastName.val().trim(),
            dateOfBirth: $dob.val(),
            householdId: Number(householdIdValue),
            membershipState: $membershipState.val()
        };

        if (!payload.firstName || !payload.lastName || !payload.dateOfBirth) {
            showMessage("Please fill out all required fields.", true);
            return;
        }

        if (id) {
            api(`/api/Persons/${id}`, "PUT", payload)
                .done(() => {
                    showMessage("Person updated.");
                    loadPersons();
                    resetForm();
                })
                .fail(xhr => showMessage(extractError(xhr), true));
        } else {
            api("/api/Persons", "POST", payload)
                .done(() => {
                    showMessage("Person created.");
                    loadPersons();
                    resetForm();
                })
                .fail(xhr => showMessage(extractError(xhr), true));
        }
    }

    function resetForm() {
        currentPerson = null;
        $personId.val("");
        $cpr.val("");
        $firstName.val("");
        $lastName.val("");
        $dob.val("");
        $householdSelect.val("");
        $membershipState.val("Active");
        $formTitle.text("Create person");
        $saveBtn.text("Create");

        $parentStatus.text("—");
        $makeParentBtn.prop("disabled", true);
        $boardRolesTBody.empty();
        $personSportsTBody.empty();
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

    loadHouseholds()
        .then(loadSports)
        .then(loadPersons)
        .catch(() => { });
})();