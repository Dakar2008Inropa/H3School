import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Suspense, lazy } from "react";
import { MainLayout } from "./layout/MainLayout/MainLayout";
import { ShowProduct } from "./components/ShowProduct/ShowProduct";

const pageModules = import.meta.glob("./pages/*.jsx");

const dynamicRoutes = Object.entries(pageModules).map(([path, importer]) => {
    const match = path.match(/\/pages\/(.+)\.jsx$/);
    const name = match ? match[1] : "Page";
    const slug = name.toLowerCase();

    const Component = lazy(() =>
        importer().then(mod => {
            const cmp = mod.default ?? mod[name];
            if (!cmp) {
                throw new Error(`Page "${name}" must export default or named "${name}".`);
            }
            return { default: cmp };
        })
    );

    if (slug === "home") {
        return { key: "index", index: true, element: <Component /> };
    }
    return { key: slug, path: slug, element: <Component /> };
});

function App() {
    return (
        <BrowserRouter>
            <Suspense fallback={<div style={{ padding: 16 }}>Loading…</div>}>
                <Routes>
                    <Route path="/" element={<MainLayout />}>
                        {dynamicRoutes.map(r =>
                            r.index
                                ? <Route key={r.key} index element={r.element} />
                                : <Route key={r.key} path={r.path} element={r.element} />
                        )}
                        <Route path="/products/:id" element={<ShowProduct />} />
                        <Route path="*" element={<div style={{ padding: 16 }}>Page not found</div>} />
                    </Route>
                </Routes>
            </Suspense>
        </BrowserRouter>
    );
}
export default App;