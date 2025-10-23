import { AboutMe } from "../components/AboutMe/AboutMe";

export const About = () => {
    return (
        <section style={{
            padding: 16,
            boxSizing: "border-box",
            width: "100%",
            display: "flex",
            alignItems: "flex-start",
            justifyContent: "center"
        }}>
            <div style={{ display: "flex", flexDirection: "column", alignItems: "center", width: "100%", gap: 16 }}>
                <AboutMe />
            </div>
        </section>
    );
}