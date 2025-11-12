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
    const $saveBtn = $("#saveBtn");

    const $parentRoleCard = $("#parentRoleCard");

    const $parentStatus = $("#parentStatus");
    const $makeParentBtn = $("#makeParentBtn");

    const $boardSportSelect = $("#boardSportSelect");
    const $boardFrom = $("#boardFrom");
    const $assignBoardRoleBtn = $("#assignBoardRoleBtn");
    const $boardRolesTBody = $("#boardRolesTable tbody");

    const $joinSportSelect = $("#joinSportSelect");
    const $joinSportBtn = $("#joinSportBtn");
    const $personSportsTBody = $("#personSportsTable tbody");

    const $membershipHistoryTBody = $("#membershipHistoryTable tbody");

    const $removeParentBtn = $("#removeParentBtn");

    const $boardRoleInfo = $("#boardRoleInfo");
    const $joinSportInfo = $("#joinSportInfo");

    let sportsCache = [];
    let sportsById = new Map();
    let currentPerson = null;

    let access = {
        isAdmin: false,
        isModerator: false,
        isUserOnly: false,
        allowCreate: false,
        allowUpdate: false,
        allowMutate: false
    };

    function updateAccessFromAuth() {
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
    }

    function applyRoleUI() {
        if (!access.allowCreate) {
            $("#createPerson").addClass("d-none");
        }
        if (access.isUserOnly) {
            $saveBtn.prop("disabled", true).addClass("disabled");
            $makeParentBtn.addClass("d-none");
            $removeParentBtn.addClass("d-none");
            $assignBoardRoleBtn.addClass("disabled").prop("disabled", true);
            $joinSportBtn.addClass("disabled").prop("disabled", true);
        }
    }

    window.PageAuthReady = function () {
        updateAccessFromAuth();
        applyRoleUI();
        loadHouseholds()
            .then(loadSports)
            .then(loadPersons)
            .catch(function () { });
        if (window.Auth) window.Auth.updateNav();
    };

    $("#reloadPersons").on("click", loadPersons);
    $("#resetBtn").on("click", resetForm);
    $("#personForm").on("submit", onSubmit);

    $makeParentBtn.on("click", onMakeParent);
    $assignBoardRoleBtn.on("click", onAssignBoardRole);
    $joinSportBtn.on("click", onJoinSport);

    $("#createPerson").on("click", onCreatePerson);

    $removeParentBtn.on("click", onRemoveParent);

    function applyCprMask(raw) {
        const digitsOnly = String(raw || "").replace(/\D/g, "").slice(0, 10);
        if (digitsOnly.length <= 6) return digitsOnly;
        return digitsOnly.slice(0, 6) + "-" + digitsOnly.slice(6);
    }

    $cpr.on("input", function () {
        const masked = applyCprMask($cpr.val());
        if ($cpr.val() !== masked) $cpr.val(masked);

        const dobIso = tryParseDobFromCpr(masked);
        if (dobIso && $dob.val() !== dobIso) {
            $dob.val(dobIso);
        }
    });

    function resolveCprYear(yy, centuryDigit) {
        if (centuryDigit >= 0 && centuryDigit <= 3) return 1900 + yy;

        if (centuryDigit === 4 || centuryDigit === 5) {
            if (yy <= 57) return 2000 + yy;
            return 1800 + yy;
        }

        if (centuryDigit >= 6 && centuryDigit <= 8) {
            if (yy <= 57) return 2000 + yy;
            return 1900 + yy;
        }

        if (centuryDigit === 9) {
            if (yy <= 36) return 2000 + yy;
            return 1900 + yy;
        }

        return null;
    }

    function tryParseDobFromCpr(masked) {
        const digits = String(masked || "").replace(/\D/g, "");

        if (digits.length >= 10) {
            const dd = Number.parseInt(digits.slice(0, 2), 10);
            const mm = Number.parseInt(digits.slice(2, 4), 10);
            const yy = Number.parseInt(digits.slice(4, 6), 10);
            const centuryDigit = Number.parseInt(digits.charAt(6), 10);

            if (!(dd >= 1 && dd <= 31) || !(mm >= 1 && mm <= 12) || Number.isNaN(centuryDigit)) return "";

            const year = resolveCprYear(yy, centuryDigit);
            if (year === null) return "";

            const mmStr = String(mm).padStart(2, "0");
            const ddStr = String(dd).padStart(2, "0");
            const iso = `${year}-${mmStr}-${ddStr}`;

            const d = new Date(iso);
            if (Number.isNaN(d.getTime())) return "";
            if ((d.getUTCFullYear() !== year) || (d.getUTCMonth() + 1 !== mm) || (d.getUTCDate() !== dd)) return "";

            return iso;
        }

        if (digits.length >= 6) {
            const dd = Number.parseInt(digits.slice(0, 2), 10);
            const mm = Number.parseInt(digits.slice(2, 4), 10);
            const yy = Number.parseInt(digits.slice(4, 6), 10);
            if (!(dd >= 1 && dd <= 31) || !(mm >= 1 && mm <= 12)) return "";

            const year = yy <= 36 ? 2000 + yy : 1900 + yy;
            const mmStr = String(mm).padStart(2, "0");
            const ddStr = String(dd).padStart(2, "0");
            const iso = `${year}-${mmStr}-${ddStr}`;

            const d = new Date(iso);
            if (Number.isNaN(d.getTime())) return "";
            if ((d.getUTCFullYear() !== year) || (d.getUTCMonth() + 1 !== mm) || (d.getUTCDate() !== dd)) return "";

            return iso;
        }

        return "";
    }

    function showMessage(text, isError = false) {
        $messages
            .text(text)
            .removeClass("error ok")
            .addClass(isError ? "error" : "ok");
        setTimeout(() => $messages.text("").removeClass("error ok"), 15000);
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
                $boardRoleInfo.text("");
                $joinSportInfo.text("");
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

    function computeAge(iso) {
        if (!iso) return "–";
        const parts = iso.split("-");
        if (parts.length < 3) return "–";
        const year = Number.parseInt(parts[0], 10);
        const month = Number.parseInt(parts[1], 10) - 1;
        const day = Number.parseInt(parts[2], 10);
        if (Number.isNaN(year) || Number.isNaN(month) || Number.isNaN(day)) return "–";
        const dob = new Date(year, month, day);
        if (Number.isNaN(dob.getTime())) return "–";
        const today = new Date();
        let age = today.getFullYear() - dob.getFullYear();
        const hasHadBirthday =
            today.getMonth() > dob.getMonth() ||
            (today.getMonth() === dob.getMonth() && today.getDate() >= dob.getDate());
        if (!hasHadBirthday) age -= 1;
        if (age < 0 || age > 130) return "–";
        return String(age);
    }

    function formatDate(iso) {
        if (!iso) return "";
        const s = String(iso);
        const match = s.match(/^(\d{4})-(\d{2})-(\d{2})/);
        if (!match) return s;
        return `${match[3]}-${match[2]}-${match[1]}`;
    }

    function isUnder18(iso) {
        const age = Number.parseInt(computeAge(iso), 10);
        if (Number.isNaN(age)) return false;
        return age < 18;
    }

    function renderPersons(list) {
        $tableBody.empty();
        if (!Array.isArray(list) || list.length === 0) {
            $tableBody.append(`<tr><td colspan="9">No persons found.</td></tr>`);
            return;
        }
        for (const p of list) {
            const ageText = computeAge(p.dateOfBirth);
            const tr = $(`
                <tr>
                    <td>${escapeHtml(p.cpr)}</td>
                    <td>${escapeHtml(p.firstName)}</td>
                    <td>${escapeHtml(p.lastName)}</td>
                    <td class="text-center" title="${escapeHtml(formatDate(p.dateOfBirt))}">${escapeHtml(ageText)}</td>
                    <td class="text-center">${p.householdId}</td>
                    <td class="text-center">${escapeHtml(p.membershipState)}</td>
                    <td class="text-center">${p.activeChildrenCount ?? 0}</td>
                    <td class="actions">
                        <div class="actions-grid">
                            <button class="edit btn btn-sm btn-outline-primary me-1" data-id="${p.id}">Edit</button>
                            <button class="delete btn btn-sm btn-outline-danger" data-id="${p.id}">Delete</button>
                        </div>
                    </td>
                </tr>
            `);
            tr.find("button.edit").on("click", () => startEdit(p.id));
            tr.find("button.delete").on("click", () => doDelete(p.id));

            if (access.isUserOnly) {
                tr.find("button.delete").addClass("d-none");
            }

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

                $formTitle.text(`Edit person #${p.id}`);
                $saveBtn.text("Update");

                updateParentUI(p);

                loadBoardRoles(p.id);

                loadPersonSports(p.id);

                loadMembershipHistory(p.id);

                if (!access.allowUpdate) {
                    $saveBtn.prop("disabled", true).addClass("disabled");
                }
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function updateParentUI(p) {
        if (isUnder18(p.dateOfBirt)) {
            $parentRoleCard.addClass("d-none");
            $parentStatus.text("—");

            $makeParentBtn.prop("disabled", true);
            $removeParentBtn.addClass("d-none").prop("disabled", true);
            return;
        }

        const under18 = p.childrenUnder18Count ?? 0;
        const status = p.hasParentRole ?
            `Yes - (Active children: ${p.activeChildrenCount ?? 0})` :
            `No - (Children under 18: ${under18})`;

        $parentStatus.text(status);

        const disableMakeParent = Boolean(p.hasParentRole) || !p.id || under18 < 1;

        $makeParentBtn.prop("disabled", disableMakeParent);

        if (p.hasParentRole && p.id) {
            $removeParentBtn.removeClass("d-none").prop("disabled", false);
        } else {
            $removeParentBtn.addClass("d-none").prop("disabled", true);
        }

        if (under18 > 0 || p.hasParentRole) {
            $parentRoleCard.removeClass("d-none");
        } else {
            $parentRoleCard.addClass("d-none");
        }

        if (access.isUserOnly) {
            $makeParentBtn.addClass("d-none");
            $removeParentBtn.addClass("d-none");
        }
    }

    function onRemoveParent() {
        if (!access.allowMutate) {
            showMessage("You are not allowed to modify.", true);
            return;
        }

        const id = $personId.val().trim();

        if (!id) {
            showMessage("Select a person first.", true);
            return;
        }

        if (!confirm("Remove parent role from this person?")) return;

        api(`/api/Persons/${id}/parent`, "DELETE")
            .done(updated => {
                showMessage("Parent role removed.");
                loadPersons();
                startEdit(updated.id);
                loadMembershipHistory(updated.id);
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function onMakeParent() {
        if (!access.allowMutate) {
            showMessage("You are not allowed to modify.", true);
            return;
        }

        const id = $personId.val().trim();
        if (!id) {
            showMessage("Select a person first.", true);
            return;
        }
        api(`/api/Persons/${id}/parent`, "POST", {})
            .done(updated => {
                currentPerson = updated;
                showMessage("Parent role created.");
                loadPersons();
                startEdit(updated.id);
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
                    setBoardAssignEnabled(access.allowMutate);
                    return;
                }
                for (const r of list) {
                    const sport = sportsById.get(r.sportId);
                    const tr = $(`
                        <tr>
                            <td>${r.id}</td>
                            <td>${escapeHtml(sport ? sport.name : String(r.sportId))}</td>
                            <td>${escapeHtml(formatDate(r.from))}</td>
                            <td>${r.to ? escapeHtml(formatDate(r.to)) : ""}</td>
                            <td>
                                <div class="actions-grid">
                                    <button class="btn btn-sm btn-outline-warning close-role btn-fullwidth" data-id="${r.id}" ${r.to ? "disabled" : ""}>Close</button>
                                </div>
                            </td>
                        </tr>
                    `);
                    tr.find(".close-role").on("click", () => closeBoardRole(r.id));
                    if (access.isUserOnly) {
                        tr.find(".close-role").addClass("d-none");
                    }
                    $boardRolesTBody.append(tr);
                }

                const hasOpen = Array.isArray(list) && list.some(x => !x.to);
                setBoardAssignEnabled(access.allowMutate && !hasOpen);
            })
            .fail(xhr => {
                $boardRolesTBody.empty().append(`<tr><td colspan="5">Failed to load roles.</td></tr>`);
                showMessage(extractError(xhr), true);
                setBoardAssignEnabled(false);
            });
    }

    function setBoardAssignEnabled(enabled) {
        $assignBoardRoleBtn.prop("disabled", !enabled);
        $boardSportSelect.prop("disabled", !enabled);

        if (enabled) {
            $boardRoleInfo.text("Select a sport to assign a board role.");

            if ($boardSportSelect.children("option").length <= 1) {
                $boardSportSelect.empty().append(`<option value="">Select sport...</option>`);
                const active = sportsCache.filter(s => s.isActive);
                for (const s of active) {
                    $boardSportSelect.append($("<option></option>").attr("value", s.id).text(s.name));
                }
            }
        } else {
            $boardRoleInfo.text("Cannot assign new board role while an open role exists.");
            $boardSportSelect.empty().append(`<option value="">Cannot assign</option>`);
        }

        if (access.isUserOnly) {
            $assignBoardRoleBtn.addClass("d-none");
        }
    }

    function onAssignBoardRole(e) {
        e.preventDefault();

        if (!access.allowMutate) {
            showMessage("You are not allowed to modify.", true);
            return;
        }

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
                startEdit(Number(personId));
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function closeBoardRole(roleId) {
        if (!access.allowMutate) {
            showMessage("You are not allowed to modify.", true);
            return;
        }

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
                    if (access.isUserOnly) {
                        $joinSportBtn.addClass("d-none");
                        $joinSportSelect.prop("disabled", true);
                        $joinSportInfo.text("Read-only access.");
                    }
                    return;
                }
                for (const ps of list) {
                    const sport = sportsById.get(ps.sportId);
                    const tr = $(`
                        <tr>
                            <td>${escapeHtml(sport ? sport.name : String(ps.sportId))}</td>
                            <td>${escapeHtml(formatDate(ps.joined))}</td>
                            <td>${ps.left ? escapeHtml(formatDate(ps.left)) : ""}</td>
                            <td>
                                <div class="actions-grid">
                                    <button class="btn btn-sm btn-outline-danger leave-sport btn-fullwidth" data-sport="${ps.sportId}" ${ps.left ? "disabled" : ""}>Leave</button>
                                </div>
                            </td>
                        </tr>
                    `);
                    tr.find(".leave-sport").on("click", () => leaveSport(personId, ps.sportId));

                    if (ps.left) {
                        tr.find(".leave-sport").addClass("d-none");
                    }

                    if (access.isUserOnly) {
                        tr.find(".leave-sport").addClass("d-none");
                    }

                    $personSportsTBody.append(tr);
                }

                const activeSportIds = Array.isArray(list) ? list.filter(x => !x.left).map(x => x.sportId) : [];
                updateJoinSportSelect(activeSportIds);
            })
            .fail(xhr => {
                $personSportsTBody.empty().append(`<tr><td colspan="4">Failed to load memberships.</td></tr>`);
                showMessage(extractError(xhr), true);
                updateJoinSportSelect([]);
                $joinSportSelect.prop("disabled", true);
                $joinSportBtn.prop("disabled", true);
                $joinSportInfo.text("Failed to load sports memberships.");
            });
    }

    function updateJoinSportSelect(activeSportIds) {
        const available = sportsCache.filter(s => s.isActive && !activeSportIds.includes(s.id));

        $joinSportSelect.empty();

        if (available.length === 0) {
            $joinSportSelect.append(`<option value="">No available sports</option>`);
            $joinSportSelect.prop("disabled", true);
            $joinSportBtn.prop("disabled", true);
            $joinSportInfo.text("No available sports to join.");
            return;
        }

        $joinSportSelect.append(`<option value="">Select sport…</option>`);
        for (const s of available) {
            $joinSportSelect.append($("<option></option>").attr("value", s.id).text(s.name));
        }

        if (access.isUserOnly) {
            $joinSportSelect.prop("disabled", true);
            $joinSportBtn.prop("disabled", true).addClass("d-none");
            $joinSportInfo.text("Read-only access.");
            return;
        }

        $joinSportSelect.prop("disabled", false);
        $joinSportBtn.prop("disabled", false);
        $joinSportInfo.text("Select a sport to join.");
    }

    function onJoinSport(e) {
        e.preventDefault();

        if (!access.allowMutate) {
            showMessage("You are not allowed to modify.", true);
            return;
        }

        const personId = $personId.val().trim();
        const sportId = Number($joinSportSelect.val());
        if (!personId || !sportId) {
            showMessage("Select a person and a sport.", true);
            return;
        }
        api(`/api/persons/${personId}/sports/join`, "POST", { sportId: sportId })
            .done(() => {
                showMessage("Joined sport.");
                loadPersons();
                loadPersonSports(Number(personId));
                startEdit(Number(personId));
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function leaveSport(personId, sportId) {
        if (!access.allowMutate) {
            showMessage("You are not allowed to modify.", true);
            return;
        }

        const now = new Date();
        const left = now.toISOString().slice(0, 10);
        const fullLeftDate = now.toISOString();
        api(`/api/persons/${personId}/sports/${sportId}/leave`, "POST", { left: left, fullLeftDate: fullLeftDate })
            .done(() => {
                showMessage("Left sport.");
                loadPersons();
                loadPersonSports(Number(personId));
            })
            .fail(xhr => showMessage(extractError(xhr), true));
    }

    function doDelete(id) {
        if (!access.allowMutate) {
            showMessage("You are not allowed to modify.", true);
            return;
        }

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

        const dobIso = tryParseDobFromCpr(cprFormatted);
        if (dobIso && !$dob.val()) {
            $dob.val(dobIso);
        }

        const payload = {
            cPR: cprFormatted,
            firstName: $firstName.val().trim(),
            lastName: $lastName.val().trim(),
            dateOfBirth: $dob.val(),
            householdId: Number(householdIdValue)
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
                    startEdit(Number(id));
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

    function loadMembershipHistory(personId) {
        $membershipHistoryTBody.empty().append(`<tr><td colspan="3">Loading...</td></tr>`);
        api(`/api/Persons/${personId}/membership-history`, "GET")
            .done(list => {
                $membershipHistoryTBody.empty();
                if (!Array.isArray(list) || list.length === 0) {
                    $membershipHistoryTBody.append(`<tr><td colspan="3">No membership history.</td></tr>`);
                    return;
                }
                for (const h of list) {
                    const tr = $(`
                        <tr>
                            <td>${escapeHtml(formatDate(h.changedOn))}</td>
                            <td class="text-center">${escapeHtml(h.state)}</td>
                            <td class="text-center">${escapeHtml(h.reason || "")}</td>
                        </tr>
                    `);
                    $membershipHistoryTBody.append(tr);
                }
            })
            .fail(xhr => {
                $membershipHistoryTBody.empty().append(`<tr><td colspan="3">Failed to load membership history.</td></tr>`);
                showMessage(extractError(xhr), true);
            });
    }

    function resetForm() {
        currentPerson = null;
        $personId.val("");
        $cpr.val("");
        $firstName.val("");
        $lastName.val("");
        $dob.val("");
        $householdSelect.val("");
        $formTitle.text("Create person");
        $saveBtn.text("Create");

        $parentStatus.text("—");
        $makeParentBtn.prop("disabled", true);
        $boardRolesTBody.empty();
        $personSportsTBody.empty();

        $parentRoleCard.addClass("d-none");

        $removeParentBtn.addClass("d-none").prop("disabled", true);

        $membershipHistoryTBody.empty();

        $boardRoleInfo.text("");
        $joinSportInfo.text("");
        $boardSportSelect.prop("disabled", false);
        $assignBoardRoleBtn.prop("disabled", false);
        $joinSportSelect.prop("disabled", false);
        $joinSportBtn.prop("disabled", false);

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

    function onCreatePerson() {
        resetForm();
        $cpr.trigger("focus");
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