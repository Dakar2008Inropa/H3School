import { ContactMe } from "../components/ContactMe/ContactMe";

export const Contact = () => {
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
                <ContactMe />
            </div>
        </section>
    );
}