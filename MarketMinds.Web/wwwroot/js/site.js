// This file contains JavaScript functions for the ASP.NET Core MVC part
// The React app has its own JavaScript in the ClientApp directory

// Wait for the document to be fully loaded
document.addEventListener("DOMContentLoaded", function () {
  console.log("MVC site.js loaded");

  // Add any global event listeners or functionality for the MVC parts here
  const navLinks = document.querySelectorAll("nav a");

  navLinks.forEach((link) => {
    link.addEventListener("mouseover", function () {
      this.style.transition = "color 0.3s";
    });
  });
});

// Example function for use in MVC views
function toggleVisibility(elementId) {
  const element = document.getElementById(elementId);
  if (element) {
    element.style.display = element.style.display === "none" ? "block" : "none";
  }
}
