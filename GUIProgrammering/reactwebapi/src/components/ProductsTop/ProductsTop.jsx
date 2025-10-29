import PropTypes from "prop-types";
import "./ProductsTop.css";

export const ProductsTop = ({
    categoryName = "Kategori",
    totalCount = 0,
    currentPage = 1,
    pageSize = 6,
    onPageChange = () => { },
}) => {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    const showPagination = totalPages > 1;

    const goToPage = (p) => {
        if (p < 1 || p > totalPages || p === currentPage) return;
        onPageChange(p);
    };

    const pages = Array.from({ length: totalPages }, (_, i) => i + 1);
    const canPrev = currentPage > 1;
    const canNext = currentPage < totalPages;

    return (
        <>
            <div className="titleContainer">
                <h1 className="catName">{categoryName}</h1>
            </div>

            <div className="metaBar">
                <div className="count">
                    {totalCount} {totalCount === 1 ? "Product" : "Products"}
                </div>

                {showPagination && (
                    <nav className="paginationContainer" aria-label="Pagination">
                        <button
                            className="paginationBtn"
                            onClick={() => goToPage(1)}
                            disabled={!canPrev}
                            aria-label="First Page"
                        >
                            «
                        </button>
                        <button
                            className="paginationBtn"
                            onClick={() => goToPage(currentPage - 1)}
                            disabled={!canPrev}
                            aria-label="Previous Page"
                        >
                            ‹
                        </button>

                        {pages.map((p) => (
                            <button
                                key={p}
                                className={`paginationBtn pageNumber ${p === currentPage ? "active" : ""
                                    }`}
                                onClick={() => goToPage(p)}
                                aria-current={p === currentPage ? "page" : undefined}
                                aria-label={`Go to page ${p}`}
                            >
                                {p}
                            </button>
                        ))}

                        <button
                            className="paginationBtn"
                            onClick={() => goToPage(currentPage + 1)}
                            disabled={!canNext}
                            aria-label="Next Page"
                        >
                            ›
                        </button>
                        <button
                            className="paginationBtn"
                            onClick={() => goToPage(totalPages)}
                            disabled={!canNext}
                            aria-label="Last Page"
                        >
                            »
                        </button>
                    </nav>
                )}
            </div>
        </>
    );
};

ProductsTop.propTypes = {
    categoryName: PropTypes.string,
    totalCount: PropTypes.number,
    currentPage: PropTypes.number,
    pageSize: PropTypes.number,
    onPageChange: PropTypes.func,
};