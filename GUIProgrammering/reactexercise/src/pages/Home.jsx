import { Carousel } from "../components/Carousel/Carousel";
import { Counter } from "../components/Counter/Counter";
import { Products } from "../components/Products/Products";

export const Home = () => {
    return (
        <section style={{
            padding: 16,
            boxSizing: 'border-box',
            height: 'calc(100dvh - var(--header-height, 72px) - var(--nav-height, 48px) - var(--footer-height, 56px))',
            overflowY: 'auto',
            overflowX: 'hidden',
            scrollbarGutter: 'stable both-edges',
            display: 'flex',
            alignItems: 'flex-start',
            justifyContent: 'center',
            width: '100%',
            paddingBottom: 'calc(var(--footer-height, 56px) + 24px)',
            ['--info-card-height']: '480px'
        }}
        >
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', width: '100%', gap: 16 }}>
                <Carousel />
                <Counter />
                { }
                <Products infoCardHeight="480px" />
            </div>
        </section>
    )
}