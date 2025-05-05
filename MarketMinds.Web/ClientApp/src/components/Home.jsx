import React from "react";
import { Link } from "react-router-dom";

function Home() {
  return (
    <div className="text-center">
      <h1 className="text-4xl font-bold mb-6">Welcome to MarketMinds</h1>
      <p className="text-xl mb-8 max-w-3xl mx-auto">
        Your ultimate marketplace for buying, selling, borrowing, and auctioning
        products. Find what you need or sell what you don't with our
        comprehensive platform.
      </p>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mt-12">
        <FeatureCard
          title="Buy Products"
          description="Browse our marketplace and find products that match your needs."
          icon="🛒"
          linkTo="/products"
        />
        <FeatureCard
          title="Sell Products"
          description="List your products and reach potential buyers easily."
          icon="💰"
          linkTo="/sell"
        />
        <FeatureCard
          title="Borrow Products"
          description="Borrow items for a specific period without having to buy them."
          icon="🔄"
          linkTo="/borrow"
        />
        <FeatureCard
          title="Auction"
          description="Participate in auctions and bid on exclusive items."
          icon="🔨"
          linkTo="/auctions"
        />
      </div>
    </div>
  );
}

function FeatureCard({ title, description, icon, linkTo }) {
  return (
    <div className="bg-white p-6 rounded-lg shadow-md hover:shadow-lg transition">
      <div className="text-4xl mb-4">{icon}</div>
      <h3 className="text-xl font-semibold mb-2">{title}</h3>
      <p className="text-gray-600 mb-4">{description}</p>
      <Link
        to={linkTo}
        className="inline-block bg-primary text-white px-4 py-2 rounded hover:bg-primary-dark transition"
      >
        Learn More
      </Link>
    </div>
  );
}

export default Home;
