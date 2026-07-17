(() => {
  "use strict";

  // Hide logo/search once page scroll passes 3%; keep cat-bar sticky.
  const initCompactHeader = () => {
    const header = document.querySelector(".site-header");
    const headerTop = header?.querySelector(".header-top");
    const catBar = header?.querySelector(".cat-bar");
    if (!header || !headerTop || !catBar) return;

    const toggle = header.querySelector(".nav-toggle");
    const mobile = header.querySelector(".nav-mobile");
    const THRESHOLD = 0.03;

    let compact = false;
    let ticking = false;

    const setCompact = (next) => {
      if (next === compact) return;
      compact = next;
      header.classList.toggle("is-compact", compact);
      headerTop.toggleAttribute("inert", compact);

      if (compact && mobile && !mobile.hasAttribute("hidden")) {
        mobile.setAttribute("hidden", "");
        mobile.classList.remove("is-open");
        toggle?.setAttribute("aria-expanded", "false");
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

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initCompactHeader, { once: true });
  } else {
    initCompactHeader();
  }
})();
