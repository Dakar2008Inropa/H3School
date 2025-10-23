import { Outlet } from "react-router-dom";
import { Navigation } from "../../components/Navigation/Navigation";
import { Header } from "../../components/Header/Header";
import { Footer } from "../../components/Footer/Footer";

export const MainLayout = () => {
    return (
        <main>
            <Header name="Daniel Vinther Andersen" />
            <Navigation />
            <Outlet />
            <Footer birth="200286" />
        </main>
    )
}