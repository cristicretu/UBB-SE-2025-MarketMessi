import React, { useState, useEffect } from "react";
import axios from "axios";

function ProductList() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // In a real app, this would fetch from your API
    // For now, we'll just use a timeout to simulate loading
    const timeout = setTimeout(() => {
      // Mock data
      const mockProducts = [
        {
          id: 1,
          name: "Smartphone XYZ",
          description: "Latest model with amazing features",
          price: 799.99,
          category: "Electronics",
          imageUrl: "https://via.placeholder.com/150",
        },
        {
          id: 2,
          name: "Designer Backpack",
          description: "Stylish and functional backpack for everyday use",
          price: 129.99,
          category: "Fashion",
          imageUrl: "https://via.placeholder.com/150",
        },
        {
          id: 3,
          name: "Wireless Headphones",
          description: "Premium sound quality with noise cancellation",
          price: 249.99,
          category: "Electronics",
          imageUrl: "https://via.placeholder.com/150",
        },
        {
          id: 4,
          name: "Coffee Maker",
          description: "Automatic coffee maker with timer",
          price: 89.99,
          category: "Home",
          imageUrl: "https://via.placeholder.com/150",
        },
      ];

      setProducts(mockProducts);
      setLoading(false);
    }, 1000);

    return () => clearTimeout(timeout);

    // In a real application, you would use:
    // axios.get('/api/products')
    //   .then(response => {
    //     setProducts(response.data);
    //     setLoading(false);
    //   })
    //   .catch(error => {
    //     setError('Failed to load products');
    //     setLoading(false);
    //     console.error('Error fetching products:', error);
    //   });
  }, []);

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center text-red-600 py-8">
        <p className="text-xl">{error}</p>
        <button
          className="mt-4 bg-primary text-white px-4 py-2 rounded"
          onClick={() => window.location.reload()}
        >
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Products</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {products.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
    </div>
  );
}

function ProductCard({ product }) {
  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition">
      <img
        src={product.imageUrl}
        alt={product.name}
        className="w-full h-48 object-cover"
      />
      <div className="p-4">
        <h3 className="text-lg font-semibold">{product.name}</h3>
        <p className="text-gray-600 text-sm mb-2">{product.category}</p>
        <p className="text-gray-700 mb-3">{product.description}</p>
        <div className="flex justify-between items-center">
          <span className="text-primary-dark font-bold">
            ${product.price.toFixed(2)}
          </span>
          <button className="bg-primary text-white px-3 py-1 rounded hover:bg-primary-dark transition">
            View Details
          </button>
        </div>
      </div>
    </div>
  );
}

export default ProductList;
