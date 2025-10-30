import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Suspense } from "react";
import { MainLayout } from "./layout/MainLayout/MainLayout";
import { Product } from "./components/Product/Product";
import { Home } from "./pages/Home";
import { Products } from "./pages/Products";
import { CartProvider } from "./cart/CartContext";
import { Cart } from "./pages/Cart";

function App() {
    return (
        <CartProvider>
            <BrowserRouter>
                <Suspense fallback={<div style={{ padding: 16 }}>Loading…</div>}>
                    <Routes>
                        <Route path="/" element={<MainLayout />}>
                            <Route path="/" element={<Home />} />
                            <Route path="/products" element={<Products />} />
                            <Route path="/products/:id" element={<Product />} />
                            <Route path="/cart" element={<Cart />} />
                            <Route path="*" element={<div style={{ padding: 16 }}>Page not found</div>} />
                        </Route>
                    </Routes>
                </Suspense>
            </BrowserRouter>
        </CartProvider>
    );
}
export default App;