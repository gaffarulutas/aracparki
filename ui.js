(() => {
  "use strict";

  const initSearchToggle = () => {
    const header = document.querySelector(".site-header");
    const btn = header?.querySelector(".search-toggle");
    const form = header?.querySelector(".header-search");
    const input = form?.querySelector("input");
    if (!header || !btn || !form) return;

    const setOpen = (open) => {
      header.classList.toggle("is-search-open", open);
      btn.setAttribute("aria-expanded", open ? "true" : "false");
      btn.setAttribute("aria-label", open ? "Aramayı kapat" : "Aramayı aç");

      if (open) {
        const navBtn = header.querySelector(".nav-toggle");
        const mobile = header.querySelector(".nav-mobile");
        if (mobile && !mobile.hasAttribute("hidden")) {
          mobile.setAttribute("hidden", "");
          mobile.classList.remove("is-open");
          navBtn?.setAttribute("aria-expanded", "false");
        }
        requestAnimationFrame(() => input?.focus());
      }
    };

    btn.addEventListener("click", () => {
      setOpen(!header.classList.contains("is-search-open"));
    });
  };

  // Scroll: collapse main header; show Logo | categories | Giriş + İlan Ver. Back to top: restore.
  const initCompactHeader = () => {
    const header = document.querySelector(".site-header");
    const headerTop = header?.querySelector(".header-top");
    const catBar = header?.querySelector(".cat-bar");
    if (!header || !headerTop || !catBar) return;

    const toggle = header.querySelector(".nav-toggle");
    const mobile = header.querySelector(".nav-mobile");
    const brand = catBar.querySelector(".cat-bar-brand");
    const THRESHOLD = 0.03;

    let compact = false;
    let ticking = false;

    const setCompact = (next) => {
      if (next === compact) return;
      compact = next;
      header.classList.toggle("is-compact", compact);
      headerTop.toggleAttribute("inert", compact);
      brand?.setAttribute("tabindex", compact ? "0" : "-1");

      if (compact) {
        header.classList.remove("is-search-open");
        const searchBtn = header.querySelector(".search-toggle");
        searchBtn?.setAttribute("aria-expanded", "false");
        searchBtn?.setAttribute("aria-label", "Aramayı aç");

        if (mobile && !mobile.hasAttribute("hidden")) {
          mobile.setAttribute("hidden", "");
          mobile.classList.remove("is-open");
          toggle?.setAttribute("aria-expanded", "false");
        }
      }
    };

    const update = () => {
      ticking = false;
      const y = Math.max(0, window.scrollY);
      const max = document.documentElement.scrollHeight - window.innerHeight;
      const progress = max > 0 ? y / max : 0;
      setCompact(progress >= THRESHOLD);
    };

    window.addEventListener(
      "scroll",
      () => {
        if (ticking) return;
        ticking = true;
        requestAnimationFrame(update);
      },
      { passive: true }
    );

    update();
  };

  const boot = () => {
    initSearchToggle();
    initCompactHeader();
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot, { once: true });
  } else {
    boot();
  }
})();
