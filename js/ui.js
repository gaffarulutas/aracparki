(() => {
  "use strict";

  const FOCUSABLE =
    'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

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
        closeMobileNav();
        requestAnimationFrame(() => input?.focus());
      }
    };

    btn.addEventListener("click", () => {
      setOpen(!header.classList.contains("is-search-open"));
    });

    document.addEventListener("keydown", (e) => {
      if (e.key !== "Escape") return;
      if (header.classList.contains("is-search-open")) {
        setOpen(false);
        btn.focus();
      }
    });
  };

  const closeMobileNav = () => {
    const header = document.querySelector(".site-header");
    const toggle = header?.querySelector(".nav-toggle");
    const mobile = header?.querySelector(".nav-mobile");
    if (!mobile || mobile.hasAttribute("hidden")) return;
    mobile.setAttribute("hidden", "");
    mobile.classList.remove("is-open");
    toggle?.setAttribute("aria-expanded", "false");
    toggle?.setAttribute("aria-label", "Menüyü aç");
  };

  const initMobileNav = () => {
    const header = document.querySelector(".site-header");
    const toggle = header?.querySelector(".nav-toggle");
    const mobile = header?.querySelector(".nav-mobile");
    if (!header || !toggle || !mobile) return;

    const setOpen = (open) => {
      if (open) {
        mobile.removeAttribute("hidden");
        header.classList.remove("is-search-open");
        const searchBtn = header.querySelector(".search-toggle");
        searchBtn?.setAttribute("aria-expanded", "false");
        searchBtn?.setAttribute("aria-label", "Aramayı aç");
        mobile.classList.add("is-open");
        toggle.setAttribute("aria-expanded", "true");
        toggle.setAttribute("aria-label", "Menüyü kapat");
        const first = mobile.querySelector(FOCUSABLE);
        requestAnimationFrame(() => first?.focus());
      } else {
        closeMobileNav();
        toggle.focus();
      }
    };

    toggle.addEventListener("click", () => {
      const open = mobile.hasAttribute("hidden");
      setOpen(open);
    });

    mobile.addEventListener("keydown", (e) => {
      if (e.key !== "Tab" || mobile.hasAttribute("hidden")) return;
      const nodes = [...mobile.querySelectorAll(FOCUSABLE)];
      if (!nodes.length) return;
      const first = nodes[0];
      const last = nodes[nodes.length - 1];
      if (e.shiftKey && document.activeElement === first) {
        e.preventDefault();
        last.focus();
      } else if (!e.shiftKey && document.activeElement === last) {
        e.preventDefault();
        first.focus();
      }
    });

    document.addEventListener("keydown", (e) => {
      if (e.key === "Escape" && !mobile.hasAttribute("hidden")) {
        setOpen(false);
      }
    });

    mobile.addEventListener("click", (e) => {
      if (e.target.closest("a")) setOpen(false);
    });
  };

  const initCompactHeader = () => {
    const header = document.querySelector(".site-header");
    const headerTop = header?.querySelector(".header-top");
    const catBar = header?.querySelector(".cat-bar");
    if (!header || !headerTop || !catBar) return;

    const brand = catBar.querySelector(".cat-bar-brand");
    const ENTER_Y = 100;
    const EXIT_Y = 36;

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
        closeMobileNav();
      }
    };

    const update = () => {
      ticking = false;
      const y = Math.max(0, window.scrollY);
      if (!compact && y >= ENTER_Y) setCompact(true);
      else if (compact && y <= EXIT_Y) setCompact(false);
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
    initMobileNav();
    initCompactHeader();
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot, { once: true });
  } else {
    boot();
  }
})();
