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

    const $toggleMasterPassword = $("#toggleMasterPassword");
    const $masterPasswordRow = $("#masterPasswordRow");

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
                $qrContainer.css({ background: "#FFFFFF", padding: "8px", borderRadius: "6px" });
                new QRCode($qrContainer[0], {
                    text: data.otpauthUri,
                    width: 300,
                    height: 300,
                    correctLevel: QRCode.CorrectLevel.Q,
                    colorDark: "#000000",
                    colorLight: "#FFFFFF"
                });
                $sharedKey.text(data.sharedKey);

                const $toggleMasterPasswordSetup = $("#toggleMasterPasswordSetup");
                const $masterPasswordSetupRow = $("#masterPasswordSetupRow");
                const $masterPasswordSetup = $("#masterPasswordSetup");
                const $btnMasterBypassSetup = $("#btnMasterBypassSetup");

                $toggleMasterPasswordSetup.off("click").on("click", function () {
                    const hidden = $masterPasswordSetupRow.hasClass("d-none");
                    if (hidden) {
                        $masterPasswordSetupRow.removeClass("d-none");
                        $toggleMasterPasswordSetup.text("Hide master password");
                        $masterPasswordSetup.trigger("focus");
                    } else {
                        $masterPasswordSetupRow.addClass("d-none");
                        $toggleMasterPasswordSetup.text("Use master password");
                        $masterPasswordSetup.val("");
                    }
                });

                $btnMasterBypassSetup.off("click").on("click", async function () {
                    const mpw = String($masterPasswordSetup.val() || "").trim();
                    if (!mpw) {
                        showError("Enter master password.");
                        $masterPasswordSetup.trigger("focus");
                        return;
                    }
                    try {
                        clearError();
                        const res = await fetch("/api/auth/2fa/bypass", {
                            method: "POST",
                            headers: { "Content-Type": "application/json", "Accept": "application/json" },
                            credentials: "include",
                            body: JSON.stringify({ masterPassword: mpw })
                        });
                        if (!res.ok) {
                            const err = await res.json().catch(() => ({}));
                            showError(err?.message || "Bypass failed.");
                            $masterPasswordSetup.trigger("focus");
                            return;
                        }
                        window.location.href = "/ui/index.html";
                    } catch (e) {
                        showError("Bypass failed.");
                        $masterPasswordSetup.trigger("focus");
                    }
                });

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
            return;
        }

        if ($formLogin.is(":visible")) {
            $userNameOrEmail.trigger("focus");
        }
    })();

    $toggleMasterPassword.on("click", function () {
        const isHidden = $masterPasswordRow.hasClass("d-none");
        if (isHidden) {
            $masterPasswordRow.removeClass("d-none");
            $twoFactorCode.prop("disabled", true);
            $toggleMasterPassword.text("Hide master password");
            $masterPassword.trigger("focus");
        } else {
            $masterPasswordRow.addClass("d-none");
            $twoFactorCode.prop("disabled", false);
            $toggleMasterPassword.text("Use master password");
            $masterPassword.val("");
            $twoFactorCode.trigger("focus");
        }
    });

    $formLogin.on("submit", async function (e) {
        e.preventDefault(); clearError();

        const payload = {
            userNameOrEmail: String($userNameOrEmail.val() || "").trim(),
            password: String($password.val() || ""),
            rememberMe: Boolean($rememberMe.prop("checked"))
        };

        if (!payload.userNameOrEmail || !payload.password) {
            showError("Please enter username/email and password.");
            $userNameOrEmail.trigger("focus");
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
                $userNameOrEmail.trigger("focus");
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
            $userNameOrEmail.trigger("focus");
        }
    });

    $form2fa.on("submit", async function (e) {
        e.preventDefault(); clearError();

        const usingMaster = !$masterPasswordRow.hasClass("d-none") && String($masterPassword.val() || "").trim().length > 0;

        const payload = {
            twoFactorCode: usingMaster ? "" : String($twoFactorCode.val() || "").trim(),
            rememberMe: Boolean($rememberMe2.prop("checked")),
            masterPassword: usingMaster ? String($masterPassword.val() || "") : ""
        };

        if (!payload.twoFactorCode && !payload.masterPassword) {
            showError("Enter code or master password.");
            if (usingMaster) $masterPassword.trigger("focus"); else $twoFactorCode.trigger("focus");
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
                if (usingMaster) $masterPassword.trigger("focus"); else $twoFactorCode.trigger("focus");
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
            if (usingMaster) $masterPassword.trigger("focus"); else $twoFactorCode.trigger("focus");
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
    $(function () {
        if ($formLogin.is(":visible")) {
            $userNameOrEmail.trigger("focus");
        }
    });
})();