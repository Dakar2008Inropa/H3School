import { Offer } from '../components/Offer/Offer';
import { FeatureProducts } from '../components/FeatureProducts/FeatureProducts';
import { BrandingBanner } from '../components/BrandingBanner/BrandingBanner';

export const Home = () => {
    return (
        <section>
            <Offer />
            <div className="main-container">
                <FeatureProducts />
                <BrandingBanner
                    imageUrl="/images/offer_image.jpg"
                    overlayColor="#2f6f73"
                    overlayOpacity={0.85}
                    overlayWidthPercent={50}
                    title="Fashion starts here."
                    subtitle="Discover refined design, soft tones, and the art of subtle sophistication."
                    highlight="Only at AURA."
                    ariaLabel="Promotional fashion banner"
                    overlaySide="right"
                />
            </div>
        </section>
    )
}