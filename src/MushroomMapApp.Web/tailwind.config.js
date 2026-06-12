/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        mushroom: {
          50: '#f7f8f6',
          100: '#ecefe9',
          200: '#d5dbce',
          300: '#b2bcab',
          400: '#8c9883',
          500: '#6a7562',
          600: '#525c4c',
          700: '#424a3e',
          800: '#383e34',
          900: '#2f342c',
          950: '#181c17',
        },
        forest: {
          50: '#f2f8f3',
          100: '#e1efe4',
          200: '#c5dfcd',
          300: '#9bc6a9',
          400: '#6ca67f',
          500: '#4b8a61',
          600: '#3a6f4d',
          700: '#30583f',
          800: '#294735',
          900: '#233c2e',
          950: '#11221a',
        }
      }
    },
  },
  plugins: [],
}
