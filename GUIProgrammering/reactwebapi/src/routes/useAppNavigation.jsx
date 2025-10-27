import { useNavigate } from "react-router-dom";

export function useAppNavigation() {
    const navigate = useNavigate();

    return {
        go: (to, options) => navigate(to, options),
        goHome: () => navigate("/"),
        goProducts: () => navigate("/products"),
        back: () => navigate(-1),
        forward: () => navigate(1),
    };
}