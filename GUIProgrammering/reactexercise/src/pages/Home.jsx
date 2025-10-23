import { useMemo } from "react";
import { InfoCard } from "../components/InfoCard/InfoCard";
import { GridContainer } from "../components/GridContainer/GridContainer";
import { Counter } from "../components/Counter/Counter";
import { Carousel } from "../components/Carousel/Carousel";

function makeProducts() {
    const fmt = new Intl.NumberFormat("da-DK");

    return Array.from({ length: 9 }, (_, i) => {
        const priceValue = Math.floor(Math.random() * (5000 - 100 + 1)) + 100;

        return {
            id: i + 1,
            title: `Product ${i + 1}`,
            imageSrc: `/images/p${i + 1}.jpg`,
            imageAlt: `Product ${i + 1} image`,
            price: `${fmt.format(priceValue)} kr`,
            description: "A short description of the product."
        };
    });
}

export const Home = () => {
    const products = useMemo(() => makeProducts(), []);
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
            paddingBottom: 'calc(var(--footer-height, 56px) + 24px)'
        }}
        >
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', width: '100%', gap: 16 }}>
                <Carousel />
                <Counter />
                <GridContainer>
                    {products.map(p => (
                        <InfoCard
                            key={p.id}
                            title={p.title}
                            imageSrc={p.imageSrc}
                            imageAlt={p.imageAlt}
                            price={p.price}
                            description={p.description}
                            onBuy={() => console.log(`Buy clicked for ${p.title}`)}
                        />
                    ))}
                </GridContainer>
            </div>
        </section>
    )
}