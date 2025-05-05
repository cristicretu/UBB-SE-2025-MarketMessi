import React from "react";
import { Link } from "react-router-dom";

function Navbar() {
  return (
    <nav className="bg-primary text-white py-4">
      <div className="container mx-auto px-4 flex justify-between items-center">
        <Link to="/" className="text-2xl font-bold">
          MarketMinds
        </Link>
        <div className="space-x-6">
          <Link to="/" className="hover:text-primary-light transition">
            Home
          </Link>
          <Link to="/products" className="hover:text-primary-light transition">
            Products
          </Link>
          <Link to="/login" className="hover:text-primary-light transition">
            Login
          </Link>
        </div>
      </div>
    </nav>
  );
}

export default Navbar;
