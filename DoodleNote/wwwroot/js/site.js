// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
	const THEME_KEY = "theme";
	const themeToggleButton = document.getElementById("themeToggleButton");

	if (!themeToggleButton) {
		return;
	}

	const setTheme = (theme) => {
		document.documentElement.setAttribute("data-bs-theme", theme);
		localStorage.setItem(THEME_KEY, theme);
		themeToggleButton.textContent = theme === "dark" ? "Light mode" : "Dark mode";
		themeToggleButton.setAttribute("aria-label", theme === "dark" ? "Switch to light mode" : "Switch to dark mode");
	};

	const currentTheme = document.documentElement.getAttribute("data-bs-theme") || "light";
	setTheme(currentTheme);

	themeToggleButton.addEventListener("click", () => {
		const nextTheme = document.documentElement.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
		setTheme(nextTheme);
	});
})();
