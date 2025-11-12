(function () {
    "use strict";

    const $btn = $("#reloadDashboard");
    const $err = $("#dashError");

    const $totalMembers = $("#totalMembers");
    const $activeMembers = $("#activeMembers");
    const $totalSports = $("#totalSports");
    const $activeSports = $("#activeSports");
    const $annualIncome = $("#annualIncome");
    const $totalHouseholds = $("#totalHouseholds");

    $btn.on("click", loadAll);

    function api(url) {
        return $.ajax({
            url: url,
            method: "GET",
            headers: { "Accept": "application/json" }
        });
    }

    function showError(message) {
        $err.removeClass("d-none").text(message || "Failed to load dashboard data.");
    }

    function hideError() {
        $err.addClass("d-none").text("");
    }

    function setLoading() {
        $totalMembers.text("…");
        $activeMembers.text("…");
        $totalSports.text("…");
        $activeSports.text("…");
        $annualIncome.text("…");
        $totalHouseholds.text("…");
    }

    function loadAll() {
        hideError();
        setLoading();

        const personsReq = api("/api/Persons")
            .then(list => {
                const total = Array.isArray(list) ? list.length : 0;
                const active = Array.isArray(list)
                    ? list.filter(p => String(p.membershipState).toLowerCase() === "active").length
                    : 0;
                $totalMembers.text(total);
                $activeMembers.text(active);
            });

        const householdsReq = api("/api/Households")
            .then(list => {
                const total = Array.isArray(list) ? list.length : 0;
                $totalHouseholds.text(total);
            });

        const sportsReq = api("/api/Sports")
            .then(list => {
                const total = Array.isArray(list) ? list.length : 0;
                const active = Array.isArray(list)
                    ? list.filter(s => s.isActive === true).length
                    : 0;
                $totalSports.text(total);
                $activeSports.text(active);
            });

        const feesReq = api("/api/Fees/annual-income")
            .then(sum => {
                const value = Number(sum);
                const formatted = Number.isFinite(value) ? value.toFixed(2) : String(sum);
                $annualIncome.text(`${formatted}`);
            });

        Promise.allSettled([personsReq, householdsReq, sportsReq, feesReq])
            .then(results => {
                const rejected = results.filter(r => r.status === "rejected");
                if (rejected.length > 0) {
                    const first = rejected[0].reason;
                    const msg = first?.responseJSON?.title || first?.statusText || "Failed to load all dashboard data.";
                    showError(msg);
                }
            });
    }

    loadAll();
})();