window.getDarkModeEnabled = () => sessionStorage.getItem("darkMode") === "true";

window.setDarkModeEnabled = (enabled) => {
    sessionStorage.setItem("darkMode", enabled ? "true" : "false");
    document.documentElement.dataset.theme = enabled ? "dark" : "light";
};

document.documentElement.dataset.theme = window.getDarkModeEnabled() ? "dark" : "light";