import { useSearchParams } from 'react-router-dom';

export const Products = () => {
    const [searchParams] = useSearchParams();
    const categoryId = searchParams.get('categoryId');

    return (
        <>
            <p>Testing {categoryId}</p>
        </>
    )
}