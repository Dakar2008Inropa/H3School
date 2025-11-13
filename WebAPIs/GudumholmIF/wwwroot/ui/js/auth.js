(function (global) {
    "use strict";

    const Auth = {
        currentUser: null,

        ensureAuth: async function (requiredRoles) {
            try {
                const res = await fetch("/api/auth/me", {
                    method: "GET",
                    headers: { "Accept": "application/json" },
                    credentials: "include"
                });
                if (!res.ok) {
                    window.location.href = "/ui/login.html";
                    return;
                }
                const me = await res.json();
                Auth.currentUser = me;

                if (me.twoFactorEnabled === false && me.twoFactorSetupPending === true && me.twoFactorBypassUsed !== true) {
                    window.location.href = "/ui/login.html";
                    return;
                }

                if (Array.isArray(requiredRoles) && requiredRoles.length > 0) {
                    const hasRole = (me.roles || []).some(r => requiredRoles.includes(r));
                    if (!hasRole) {
                        window.location.href = "/ui/index.html";
                        return;
                    }
                }

                Auth.updateNav();
            } catch {
                window.location.href = "/ui/login.html";
            }
        },

        updateNav: function () {
            const userNameEl = document.getElementById("navUserName");
            const adminLink = document.getElementById("navAdmin");
            const signOutBtn = document.getElementById("btnSignOut");

            if (userNameEl && Auth.currentUser) {
                userNameEl.textContent = Auth.currentUser.userName || Auth.currentUser.email || "User";
                userNameEl.classList.remove("d-none");
            }
            if (adminLink) {
                const isAdmin = !!Auth.currentUser && Array.isArray(Auth.currentUser.roles) && Auth.currentUser.roles.includes("Administrator");
                if (isAdmin) adminLink.classList.remove("d-none");
                else adminLink.classList.add("d-none");
            }
            if (signOutBtn) {
                signOutBtn.addEventListener("click", async function () {
                    try {
                        await fetch("/api/auth/logout", { method: "POST", credentials: "include" });
                    } finally {
                        window.location.href = "/ui/login.html";
                    }
                }, { once: true });
            }
        }
    };

    global.Auth = Auth;
})(window);