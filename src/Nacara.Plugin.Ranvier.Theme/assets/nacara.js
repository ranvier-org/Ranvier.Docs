/* Adapted from Nacara.Theme.Default; Apache-2.0. See THIRD-PARTY-NOTICES.md. */
(()=>{
const STORAGE_KEY = "nacara-theme";

const dark = matchMedia("(prefers-color-scheme: dark)");

const chosen = () => {
    try {
        return localStorage.getItem(STORAGE_KEY) || "system";
    } catch {
        return "system"; /* private mode */
    }
};

const settle = (setting) => {
    const root = document.documentElement;

    root.dataset.themeSetting = setting;
    root.dataset.theme = setting === "system" ? (dark.matches ? "dark" : "light") : setting;
};

const themePicker = () => {
    dark.addEventListener("change", () => {
        if (document.documentElement.dataset.themeSetting === "system") settle("system");
    });

    for (const select of document.querySelectorAll("[data-nacara-theme]")) {
        select.value = chosen();

        select.addEventListener("change", () => {
            settle(select.value);

            try {
                localStorage.setItem(STORAGE_KEY, select.value);
            } catch {
                /* private mode */
            }
        });
    }
};

const copyButtons = () => {
    const announcer = document.createElement("div");
    announcer.setAttribute("role", "status");
    announcer.className = "nacara-visually-hidden";
    document.body.append(announcer);

    // The Clipboard API exists only in a secure context, which a site served over plain http is not.
    const write = async (text) => {
        if (navigator.clipboard?.writeText) {
            await navigator.clipboard.writeText(text);
            return true;
        }

        const staging = document.createElement("textarea");
        staging.value = text;
        staging.setAttribute("readonly", "");
        staging.style.position = "fixed";
        staging.style.top = "0";
        staging.style.opacity = "0";
        document.body.append(staging);
        staging.select();

        try {
            return document.execCommand("copy");
        } finally {
            staging.remove();
        }
    };

    document.addEventListener("click", async (event) => {
        const button = event.target.closest(".nacara-code__copy");
        if (!button) return;

        // A block that folds some of its lines is drawn as more than one <pre>.
        const block = button.closest(".nacara-code");
        const text =
            block?.dataset.source ??
            [...(button.closest(".nacara-code__body")?.querySelectorAll("code") ?? [])]
                .map((node) => node.textContent)
                .join("");

        if (!text) return;

        let copied = false;

        try {
            copied = await write(text.replace(/\n$/, ""));
        } catch {
            copied = false;
        }

        const said = copied ? "Copied" : "Press Ctrl+C to copy";
        button.dataset.copied = String(copied);
        button.dataset.feedback = said;
        announcer.textContent = said;

        setTimeout(
            () => {
                delete button.dataset.copied;
                delete button.dataset.feedback;
                announcer.textContent = "";
            },
            copied ? 1500 : 3000,
        );
    });
};

const OPEN_DELAY = 120;
const CLOSE_DELAY = 250;

const dropdowns = () => {
    // Touch has no hover, and a hybrid laptop would spend its first tap on one.
    const hoverable = matchMedia("(hover: hover) and (pointer: fine)");

    const setOpen = (dropdown, open) => {
        dropdown.dataset.open = String(open);

        if (!open) delete dropdown.dataset.pinned;

        dropdown
            .querySelector("[data-nacara-dropdown]")
            ?.setAttribute("aria-expanded", String(open));
    };

    const closeAll = (except) => {
        for (const dropdown of document.querySelectorAll('.nacara-dropdown[data-open="true"]')) {
            if (dropdown !== except) setOpen(dropdown, false);
        }
    };

    document.addEventListener("click", (event) => {
        const trigger = event.target.closest("[data-nacara-dropdown]");
        const dropdown = trigger?.closest(".nacara-dropdown");

        closeAll(dropdown);

        if (!dropdown) return;

        // Clicking one the pointer already opened keeps it: that is what a click adds to a hover.
        if (dropdown.dataset.open === "true" && dropdown.dataset.pinned !== "true") {
            dropdown.dataset.pinned = "true";
            return;
        }

        setOpen(dropdown, dropdown.dataset.open !== "true");

        if (dropdown.dataset.open === "true") dropdown.dataset.pinned = "true";
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") closeAll();
    });

    for (const dropdown of document.querySelectorAll(".nacara-dropdown")) {
        let timer = null;

        const after = (delay, action) => {
            clearTimeout(timer);
            timer = setTimeout(action, delay);
        };

        dropdown.addEventListener("mouseenter", () => {
            if (!hoverable.matches) return;

            after(OPEN_DELAY, () => {
                closeAll(dropdown);
                setOpen(dropdown, true);
            });
        });

        dropdown.addEventListener("mouseleave", () => {
            if (!hoverable.matches) return;

            clearTimeout(timer);

            if (dropdown.dataset.pinned === "true") return;

            after(CLOSE_DELAY, () => setOpen(dropdown, false));
        });
    }
};

const mobileMenu = () => {
    const drawer = document.querySelector(".nacara-sidebar");

    if (!drawer) return;

    const setOpen = (open) => {
        drawer.dataset.open = String(open);

        for (const toggle of document.querySelectorAll("[data-nacara-menu-toggle]")) {
            toggle.setAttribute("aria-expanded", String(open));
        }
    };

    document.addEventListener("click", (event) => {
        const toggle = event.target.closest("[data-nacara-menu-toggle]");

        if (toggle) {
            setOpen(drawer.dataset.open !== "true");
            return;
        }

        // The scrim belongs to the layout, so anything landing on it is outside the drawer.
        if (drawer.dataset.open === "true" && !event.target.closest(".nacara-sidebar")) {
            setOpen(false);
        }
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && drawer.dataset.open === "true") setOpen(false);
    });
};

// The breakpoint is the one responsive.css hides the navbar's links at.
const narrowWidgets = () => {
    const end = document.querySelector(".nacara-navbar__items--end");
    const sidebar = document.querySelector(".nacara-sidebar");

    if (!end || !sidebar) return;

    const movable = [...end.children].filter((item) => !item.querySelector("[data-nacara-search]"));

    if (movable.length === 0) return;

    const shelf = document.createElement("ul");
    shelf.className = "nacara-sidebar__widgets";

    const narrow = matchMedia("(max-width: 860px)");
    let placed = null;

    const place = () => {
        if (placed === narrow.matches) return;

        placed = narrow.matches;

        if (narrow.matches) {
            sidebar.append(shelf);

            for (const item of movable) shelf.append(item);
        } else {
            for (const item of movable) end.append(item);

            shelf.remove();
        }
    };

    place();
    narrow.addEventListener("change", place);
};

const scrollSpy = () => {
    const entries = [...document.querySelectorAll(".nacara-toc__link")]
        .map((link) => ({
            link,
            heading: document.getElementById(decodeURIComponent(link.hash.slice(1))),
        }))
        .filter((entry) => entry.heading);

    if (entries.length === 0) return;

    let active = null;
    let pinned = null;

    const setActive = (link) => {
        if (link === active) return;

        if (active) delete active.dataset.active;
        if (link) link.dataset.active = "true";

        active = link;
    };

    const update = () => {
        if (pinned) {
            setActive(pinned);
            return;
        }

        const bar = document.querySelector(".nacara-navbar");
        const line = (bar ? bar.getBoundingClientRect().height : 56) + 24;

        const atBottom =
            Math.ceil(scrollY + innerHeight) >= document.documentElement.scrollHeight - 2;

        if (atBottom) {
            setActive(entries[entries.length - 1].link);
            return;
        }

        let current = entries[0];

        for (const entry of entries) {
            if (entry.heading.getBoundingClientRect().top - line > 1) break;

            current = entry;
        }

        setActive(current.link);
    };

    let queued = false;

    const onScroll = () => {
        if (queued) return;

        queued = true;

        requestAnimationFrame(() => {
            queued = false;
            update();
        });
    };

    const pinFromHash = () => {
        if (!location.hash) return;

        const wanted = decodeURIComponent(location.hash.slice(1));
        const entry = entries.find((entry) => entry.heading.id === wanted);

        if (entry) {
            pinned = entry.link;
            setActive(entry.link);
        }
    };

    for (const entry of entries) {
        entry.link.addEventListener("click", () => {
            pinned = entry.link;
            setActive(entry.link);
        });
    }

    const release = () => {
        pinned = null;
    };

    addEventListener("wheel", release, { passive: true });
    addEventListener("touchmove", release, { passive: true });
    addEventListener("keydown", (event) => {
        if (/^(Arrow|Page)|^(Home|End|Space| )$/.test(event.key)) release();
    });

    addEventListener("scroll", onScroll, { passive: true });
    addEventListener("resize", onScroll);
    addEventListener("hashchange", () => {
        pinFromHash();
        onScroll();
    });

    update();
    pinFromHash();
};

let groups = 0;

class NacaraTabs extends HTMLElement {
    connectedCallback() {
        // Moving the element around the document connects it again.
        if (this.dataset.upgraded === "true") return;

        const panels = [...this.querySelectorAll(":scope > nacara-tab")];
        if (panels.length === 0) return;

        this.dataset.upgraded = "true";

        const group = ++groups;
        const list = document.createElement("div");
        list.className = "nacara-tabs__list";
        list.setAttribute("role", "tablist");

        const buttons = panels.map((panel, index) => {
            const button = document.createElement("button");
            button.type = "button";
            button.id = `nacara-tab-${group}-${index}`;
            button.className = "nacara-tabs__tab";
            button.textContent = panel.dataset.label || `Tab ${index + 1}`;
            button.setAttribute("role", "tab");

            panel.id ||= `nacara-tabpanel-${group}-${index}`;
            panel.setAttribute("role", "tabpanel");
            panel.setAttribute("aria-labelledby", button.id);
            button.setAttribute("aria-controls", panel.id);

            button.addEventListener("click", () => this.select(index));
            list.append(button);

            return button;
        });

        this.buttons = buttons;
        this.panels = panels;
        this.prepend(list);
        this.select(this.restoreIndex());

        list.addEventListener("keydown", (event) => {
            const delta = event.key === "ArrowRight" ? 1 : event.key === "ArrowLeft" ? -1 : 0;
            if (delta === 0) return;

            event.preventDefault();

            const current = buttons.findIndex((button) => button.tabIndex === 0);
            const next = (current + delta + buttons.length) % buttons.length;

            this.select(next);
            buttons[next].focus();
        });
    }

    restoreIndex() {
        const key = this.dataset.sync;

        if (!key) return 0;

        const stored = sessionStorage.getItem(`nacara-tabs:${key}`);
        const index = this.panels.findIndex((panel) => panel.dataset.label === stored);

        return index === -1 ? 0 : index;
    }

    select(index) {
        this.panels.forEach((panel, current) => {
            panel.hidden = current !== index;
        });

        this.buttons.forEach((button, current) => {
            button.setAttribute("aria-selected", String(current === index));
            button.tabIndex = current === index ? 0 : -1;
        });

        const key = this.dataset.sync;
        const label = this.panels[index]?.dataset.label;
        if (!key || !label) return;

        sessionStorage.setItem(`nacara-tabs:${key}`, label);

        for (const other of document.querySelectorAll(
            `nacara-tabs[data-sync="${CSS.escape(key)}"]`,
        )) {
            if (other === this) continue;

            const target = other.panels?.findIndex((panel) => panel.dataset.label === label);

            if (target > -1) other.select(target);
        }
    }
}

const foldMemory = () => {
    const keyOf = (group) => `nacara-menu:${group.dataset.nacaraMenuGroup}`;

    for (const group of document.querySelectorAll("[data-nacara-menu-group]")) {
        if (group.closest('[data-nacara-menu-memory="false"]')) continue;

        const remembered = sessionStorage.getItem(keyOf(group));

        if (remembered !== null && !group.querySelector('[aria-current="page"]')) {
            group.open = remembered === "true";
        }

        group.addEventListener("toggle", () => {
            sessionStorage.setItem(keyOf(group), String(group.open));
        });
    }
};

/** One entry of a menu, as the markup the sidebar writes for it. */
const entryOf = (node) => {
    const item = document.createElement("li");
    item.dataset.nacaraMenuAdded = "";
    item.hidden = true;

    const link = document.createElement("a");
    link.href = node.url;
    link.textContent = node.label;

    if (!node.children) {
        link.className = "nacara-sidebar__link";
        item.append(link);

        return item;
    }

    link.className = "nacara-sidebar__group-link";

    const group = document.createElement("details");
    group.className = "nacara-sidebar__group";
    group.dataset.nacaraMenuGroup = node.label;

    const summary = document.createElement("summary");
    summary.className = "nacara-sidebar__group-title";
    summary.append(link);

    const list = document.createElement("ul");
    list.className = "nacara-sidebar__list";
    list.append(...node.children.map(entryOf));

    group.append(summary, list);
    item.append(group);

    return item;
};

const linkOf = (item) => item.querySelector(":scope > a, :scope > details > summary > a");

/** The entries a pruned page left out, put back where they belong. */
const graft = (list, nodes) => {
    if (!list) return;

    const present = new Map(
        [...list.children].map((item) => [linkOf(item)?.getAttribute("href"), item]),
    );

    for (const node of nodes) {
        const item = present.get(node.url);

        if (!item) {
            list.append(entryOf(node));
            continue;
        }

        if (node.children) {
            graft(item.querySelector(":scope > details > ul"), node.children);
        }
    }
};

const menuFilter = (box) => {
    const sidebar = box.closest(".nacara-sidebar");
    if (!sidebar) return;

    const folded = new Map();
    const source = box.dataset.nacaraMenuSource;
    let grafted = null;

    const labelOf = (item) => {
        const own = item.querySelector(":scope > a, :scope > details > summary");

        return (own ? own.textContent : "").trim().toLowerCase();
    };

    // The menu on a page of a large section holds a part of it. The rest is fetched once and put
    // into the tree hidden, so filtering it is filtering an ordinary menu.
    const whole = () => {
        if (!source) return Promise.resolve();

        grafted =
            grafted ||
            fetch(source)
                .then((answer) => answer.json())
                .then((nodes) => graft(sidebar.querySelector(".nacara-sidebar__list"), nodes))
                .catch(() => {});

        return grafted;
    };

    const apply = () => {
        const term = box.value.trim().toLowerCase();
        const items = [...sidebar.querySelectorAll("li")];

        if (term === "") {
            for (const item of items) item.hidden = item.dataset.nacaraMenuAdded !== undefined;

            for (const [group, open] of folded) group.open = open;

            folded.clear();
            return;
        }

        // Deepest first, so a group can ask whether anything under it survived.
        for (const item of [...items].reverse()) {
            const kept = [...item.querySelectorAll(":scope li")].some((child) => !child.hidden);

            item.hidden = !labelOf(item).includes(term) && !kept;

            const group = item.querySelector(":scope > details");

            if (group && !item.hidden) {
                if (!folded.has(group)) folded.set(group, group.open);

                group.open = true;
            }
        }
    };

    box.addEventListener("focus", whole);

    box.addEventListener("input", async () => {
        await whole();
        apply();
    });

    box.addEventListener("keydown", (event) => {
        if (event.key === "Escape" && box.value !== "") {
            event.stopPropagation();
            box.value = "";
            box.dispatchEvent(new Event("input"));
        }
    });
};


customElements.define("nacara-tabs", NacaraTabs);
customElements.define("nacara-tab", class extends HTMLElement {});

themePicker();
copyButtons();
dropdowns();
mobileMenu();
narrowWidgets();
scrollSpy();
foldMemory();

for (const box of document.querySelectorAll("[data-nacara-menu-filter]")) menuFilter(box);

})();