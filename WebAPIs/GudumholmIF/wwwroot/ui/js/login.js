(function () {
    "use strict";

    const $loginError = $("#loginError");

    const $formLogin = $("#formLogin");
    const $userNameOrEmail = $("#userNameOrEmail");
    const $password = $("#password");
    const $rememberMe = $("#rememberMe");

    const $form2fa = $("#form2fa");
    const $twoFactorCode = $("#twoFactorCode");
    const $masterPassword = $("#masterPassword");
    const $rememberMe2 = $("#rememberMe2");

    const $setup2fa = $("#setup2fa");
    const $qrContainer = $("#qrContainer");
    const $sharedKey = $("#sharedKey");
    const $formConfirm2fa = $("#formConfirm2fa");
    const $confirmCode = $("#confirmCode");

    function showError(msg) {
        $loginError.text(msg || "Unexpected error.").removeClass("d-none");
    }

    function clearError() {
        $loginError.addClass("d-none").empty();
    }

    async function getMe() {
        try {
            const res = await fetch("/api/auth/me", { credentials: "include" });
            if (!res.ok) return null;
            return await res.json();
        } catch {
            return null;
        }
    }

    async function maybeShowSetup2fa() {
        const me = await getMe();
        if (!me) return;

        if (me.twoFactorEnabled === false && me.twoFactorSetupPending === true) {
            try {
                const res = await fetch("/api/auth/2fa/setup", {
                    method: "POST",
                    headers: { "Accept": "application/json" },
                    credentials: "include"
                });
                if (!res.ok) {
                    showError("Failed to prepare two-factor setup.");
                    return;
                }
                const data = await res.json();
                $qrContainer.empty();
                new QRCode($qrContainer[0], {
                    text: data.otpauthUri,
                    width: 180,
                    height: 180,
                    correctLevel: QRCode.CorrectLevel.M
                });
                $sharedKey.text(data.sharedKey);
                $setup2fa.removeClass("d-none");
                $formLogin.addClass("d-none");
                $form2fa.addClass("d-none");
            } catch {
                showError("Failed to prepare two-factor setup.");
            }
        } else {
            window.location.href = "/ui/index.html";
        }
    }

    (async function init() {
        clearError();
        const me = await getMe();
        if (me) {
            if (me.twoFactorEnabled === false && me.twoFactorSetupPending === true) {
                await maybeShowSetup2fa();
                return;
            }
            window.location.href = "/ui/index.html";
        }
    })();

    $formLogin.on("submit", async function (e) {
        e.preventDefault(); clearError();

        const payload = {
            userNameOrEmail: String($userNameOrEmail.val() || "").trim(),
            password: String($password.val() || ""),
            rememberMe: Boolean($rememberMe.prop("checked"))
        };

        if (!payload.userNameOrEmail || !payload.password) {
            showError("Please enter username/email and password.");
            return;
        }

        try {
            const res = await fetch("/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json", "Accept": "application/json" },
                credentials: "include",
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                const err = await res.json().catch(() => ({}));
                showError(err?.message || "Invalid credentials.");
                return;
            }

            const data = await res.json();
            if (data && data.requiresTwoFactor === true) {
                $form2fa.removeClass("d-none");
                $formLogin.addClass("d-none");
                $twoFactorCode.trigger("focus");
                return;
            }

            if (data && data.twoFactorEnabled === false && data.twoFactorSetupPending === true) {
                await maybeShowSetup2fa();
                return;
            }

            window.location.href = "/ui/index.html";
        } catch {
            showError("Login failed.");
        }
    });

    $form2fa.on("submit", async function (e) {
        e.preventDefault(); clearError();

        const payload = {
            twoFactorCode: String($twoFactorCode.val() || "").trim(),
            rememberMe: Boolean($rememberMe2.prop("checked")),
            masterPassword: String($masterPassword.val() || "")
        };

        if (!payload.twoFactorCode && !payload.masterPassword) {
            showError("Enter code or master password.");
            return;
        }

        try {
            const res = await fetch("/api/auth/login/2fa", {
                method: "POST",
                headers: { "Content-Type": "application/json", "Accept": "application/json" },
                credentials: "include",
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                const err = await res.json().catch(() => ({}));
                showError(err?.message || "Two-factor verification failed.");
                return;
            }

            const data = await res.json();
            if (data && data.twoFactorEnabled === false && data.twoFactorSetupPending === true) {
                await maybeShowSetup2fa();
                return;
            }

            window.location.href = "/ui/index.html";
        } catch {
            showError("Two-factor verification failed.");
        }
    });

    $formConfirm2fa.on("submit", async function (e) {
        e.preventDefault(); clearError();

        const code = String($confirmCode.val() || "").trim();
        if (!code) {
            showError("Enter the first code from your authenticator app.");
            return;
        }

        try {
            const res = await fetch("/api/auth/2fa/confirm", {
                method: "POST",
                headers: { "Content-Type": "application/json", "Accept": "application/json" },
                credentials: "include",
                body: JSON.stringify({ twoFactorCode: code })
            });
            if (!res.ok) {
                const err = await res.json().catch(() => ({}));
                showError(err?.message || "Failed to enable two-factor.");
                return;
            }
            window.location.href = "/ui/index.html";
        } catch {
            showError("Failed to enable two-factor.");
        }
    });
})();