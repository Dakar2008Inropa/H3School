import { useNavigate } from "react-router-dom";

export function useAppNavigation() {
    const navigate = useNavigate();

    return {
        go: (to, options) => navigate(to, options),
        goHome: () => navigate("/"),
        goAbout: () => navigate("/about"),
        goContact: () => navigate("/contact"),
        back: () => navigate(-1),
        forward: () => navigate(1),
    };
}