/** @type {import('tailwindcss').Config} */
export default {
  content: ["./src/**/*.{js,jsx,ts,tsx}", "../Views/**/*.cshtml"],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: "#3b82f6", // blue-500
          dark: "#2563eb", // blue-600
          light: "#60a5fa", // blue-400
        },
        secondary: {
          DEFAULT: "#6b7280", // gray-500
          dark: "#4b5563", // gray-600
          light: "#9ca3af", // gray-400
        },
      },
    },
  },
  plugins: [],
};
