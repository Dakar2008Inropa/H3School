import { Header } from "./components/Header/Header";
import { InfoCard } from "./components/InfoCard/InfoCard";
import { Footer } from "./components/Footer/Footer";

import myPhoto from './assets/me.jpg';

function App() {
    return (
        <>
            <Header name="Daniel Vinther Andersen" />
            <main style={{
                padding: 16,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                minHeight: 'calc(100vh - var(--header-height, 72px) - var(--footer-height, 56px))'
            }}>
                <InfoCard
                    name="Daniel Vinther Andersen"
                    imageSrc={myPhoto}
                    imageAlt="Photo of Daniel Vinther Andersen"
                    hobbies={['Coding', 'Family', 'Bowling']}
                />
            </main>
            <Footer birth="200286" />
        </>
    )
}

export default App