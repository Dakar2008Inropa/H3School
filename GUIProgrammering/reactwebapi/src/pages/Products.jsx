import { useEffect, useState, useRef } from "react";
import { useSearchParams } from "react-router-dom";
import { ProductCards } from "../components/ProductCards/ProductCards";
import { ProductsTop } from "../components/ProductsTop/ProductsTop";
import { ProductsApi } from "../api/productsApi";

function toInt(v) {
  if (v === null || v === undefined) return null;
  if (typeof v === "string") {
    const s = v.trim().toLowerCase();
    if (s === "" || s === "null" || s === "undefined") return null;
  }
  const n = Number(v);
  return Number.isFinite(n) ? n : null;
}

function mapProduct(p) {
  const id = p?.id ?? p?.productId ?? 0;
  const title = p?.name ?? p?.title ?? "Untitled";
  const price = p?.price ?? 0;
  const image = typeof p?.imageUrl === "string" ? p.imageUrl : "";

  const categoryId =
    p?.categoryId ??
    p?.categoryID ??
    p?.CategoryId ??
    p?.category?.id ??
    p?.category?.categoryId ??
    p?.category?.categoryID ??
    null;

  const categoryName =
    p?.categoryName ?? p?.category?.name ?? p?.category?.title ?? null;

  return { id, title, price, image, categoryId, categoryName };
}

export const Products = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  const categoryIdParam = searchParams.get("categoryId");
  const categoryNameParam = searchParams.get("categoryName");

  const initialPage = toInt(searchParams.get("pageNumber")) || 1;

  const selectedCategoryId = toInt(categoryIdParam);
  const selectedCategoryNameRaw = (categoryNameParam || "")
    .trim()
    .toLowerCase();
  const isAllProductsName = [
    "all",
    "all products",
    "alle",
    "alle produkter",
  ].includes(selectedCategoryNameRaw);

  const [pageNumber, setPageNumber] = useState(initialPage);
  const [pageSize, setPageSize] = useState(6);

  const [items, setItems] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const prevCategoryKey = useRef(null);
  useEffect(() => {
    const key = `${
      isAllProductsName ? "all" : "id:" + (selectedCategoryId ?? "none")
    }|name:${selectedCategoryNameRaw}`;
    const prev = prevCategoryKey.current;
    prevCategoryKey.current = key;
    if (prev === null) return;
    if (prev !== key) {
      setPageNumber(1);
      const sp = new URLSearchParams(searchParams);
      sp.set("pageNumber", "1");
      setSearchParams(sp, { replace: true });
    }
  }, [selectedCategoryId, selectedCategoryNameRaw, isAllProductsName]);

  useEffect(() => {
    const sp = new URLSearchParams(searchParams);
    const raw = sp.get("pageNumber");
    const current = toInt(raw);
    if (raw === null || current !== pageNumber) {
      sp.set("pageNumber", String(pageNumber));
      setSearchParams(sp);
    }
  }, [pageNumber, searchParams, setSearchParams]);

  useEffect(() => {
    const p = toInt(searchParams.get("pageNumber")) || 1;
    if (p !== pageNumber) setPageNumber(p);
  }, [searchParams]);

  useEffect(() => {
    const ac = new AbortController();

    async function load() {
      try {
        setLoading(true);
        setError("");

        const apiCategoryId =
          !isAllProductsName && selectedCategoryId !== null
            ? selectedCategoryId
            : undefined;

        const { items: raw, paging } = await ProductsApi.listPaged(
          { categoryId: apiCategoryId, pageNumber, pageSize },
          ac.signal
        );

        const list = Array.isArray(raw) ? raw.map(mapProduct) : [];
        setItems(list);

        const serverPageNumber = paging?.pageNumber;
        const serverPageSize = paging?.pageSize;
        const serverTotalCount = paging?.totalCount;

        if (Number.isFinite(serverTotalCount)) setTotalCount(serverTotalCount);

        if (
          Number.isFinite(serverPageNumber) &&
          serverPageNumber !== pageNumber
        ) {
          setPageNumber(serverPageNumber);
        }
        if (Number.isFinite(serverPageSize) && serverPageSize !== pageSize) {
          setPageSize(serverPageSize);
        }
      } catch (e) {
        if (e?.name === "AbortError") return;
        setError(e?.message || "Could not get products...");
      } finally {
        setLoading(false);
      }
    }

    load();
    return () => ac.abort();
  }, [isAllProductsName, selectedCategoryId, pageNumber, pageSize]);

  const displayCategoryName =
    isAllProductsName || (!categoryNameParam && selectedCategoryId === null)
      ? "All Products"
      : categoryNameParam || "Products";

  return (
    <>
      <div className="main-container">
        <ProductsTop
          categoryName={displayCategoryName}
          totalCount={totalCount}
          currentPage={pageNumber}
          pageSize={pageSize}
          onPageChange={setPageNumber}
        />

        <ProductCards items={items} loading={loading} error={error} />
      </div>
    </>
  );
};
