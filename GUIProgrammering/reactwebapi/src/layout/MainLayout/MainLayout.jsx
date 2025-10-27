import { Outlet } from "react-router-dom";
import { Navigation } from "../../components/Navigation/Navigation";
import { Header } from "../../components/Header/Header";
import { Footer } from "../../components/Footer/Footer";

export const MainLayout = () => {
    return (
        <>
            <Header titlePart1="Aura" titlePart2="Edit" />
            <Navigation />
            <div className="content-wrapper">
                <Outlet />
            </div>
            <Footer />
        </>
    )
}