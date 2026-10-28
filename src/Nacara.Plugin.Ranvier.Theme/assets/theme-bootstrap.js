(() => {
    let setting = "system";
    try { setting = localStorage.getItem("nacara-theme") || "system"; } catch {}
    const root = document.documentElement;
    root.dataset.themeSetting = setting;
    root.dataset.theme = setting === "system"
        ? (matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light")
        : setting;
})();
