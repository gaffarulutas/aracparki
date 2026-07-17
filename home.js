(() => {
  "use strict";

  const {
    ILLER,
    POPULAR_CITIES,
    CATEGORIES,
    LISTINGS,
    escapeHtml,
    formatPrice,
    formatHours,
    formatTons,
    formatHp,
    badgeLabel,
    getListing,
    matchesListing,
    readUrlState,
    buildListUrl,
    detailUrl,
    iconSvg,
    STORAGE_RECENT,
    loadJson,
    saveJson,
  } = window.AP;

  const MAX_RECENT = 6;
  let state = { ...readUrlState(), filter: readUrlState().filter || "all" };
  // Homepage showcase defaults to all unless URL has params
  if (!window.location.search) {
    state = { filter: "all", category: "", city: "", query: "", sort: "yeni" };
  }

  let recent = loadJson(STORAGE_RECENT, []);

  const showToast = (message) => {
    const toast = document.getElementById("toast");
    if (!toast) return;
    toast.hidden = false;
    toast.textContent = message;
    clearTimeout(showToast._t);
    showToast._t = setTimeout(() => {
      toast.hidden = true;
    }, 2200);
  };

  const goToList = () => {
    window.location.href = buildListUrl(state);
  };

  const cardHtml = (listing) => {
    const badge = badgeLabel(listing);
    const title = escapeHtml(listing.title);
    const href = detailUrl(listing.id);
    const price = listing.priceUnit
      ? `${formatPrice(listing.price)}<small>/ ${escapeHtml(listing.priceUnit)}</small>`
      : formatPrice(listing.price);
    return `
      <article class="listing-card is-visible" data-id="${listing.id}">
        <div class="listing-media">
          <span class="${escapeHtml(badge.className)}">${escapeHtml(badge.text)}</span>
          <a href="${href}"><img src="${escapeHtml(listing.image)}" alt="${title}" loading="lazy" width="900" height="620" /></a>
        </div>
        <div class="listing-body">
          <h3><a href="${href}">${title}</a></h3>
          <div class="listing-meta">
            <span>${listing.year}</span>
            <span>${escapeHtml(listing.city)} / ${escapeHtml(listing.district)}</span>
          </div>
          <div class="listing-specs">
            <span>${formatHours(listing.hours)}</span>
            <span>${formatTons(listing.tons)}</span>
            <span>${formatHp(listing.hp)}</span>
          </div>
          <div class="listing-foot">
            <div class="listing-price">${price}</div>
            <span class="seller-tag${listing.verified ? " verified" : ""}">${listing.verified ? "Doğrulanmış · " : ""}${escapeHtml(listing.seller)}</span>
          </div>
          <div class="listing-cta">
            <a class="btn btn-machine btn-sm" href="${href}">İncele</a>
            <a class="btn btn-ghost btn-sm" href="tel:${escapeHtml(listing.phone.replace(/\s/g, ""))}">Telefon</a>
          </div>
        </div>
      </article>`;
  };

  const renderListings = () => {
    const grid = document.getElementById("listing-grid");
    const countEl = document.getElementById("result-count");
    if (!grid) return;
    const items = LISTINGS.filter((item) => matchesListing(item, state)).slice(0, 12);
    if (countEl) countEl.textContent = `(${items.length})`;
    if (!items.length) {
      grid.innerHTML = '<p class="listing-empty">Bu kriterlere uygun ilan yok. <a href="ilanlar.html">Tüm ilanlar</a></p>';
      return;
    }
    grid.innerHTML = items.map((item) => cardHtml(item)).join("");
  };

  const renderRecent = () => {
    const section = document.getElementById("recent");
    const grid = document.getElementById("recent-grid");
    if (!section || !grid) return;
    const items = recent.map(getListing).filter(Boolean);
    if (!items.length) {
      section.hidden = true;
      return;
    }
    section.hidden = false;
    grid.innerHTML = items.map((item) => cardHtml(item)).join("");
  };

  const renderCategories = () => {
    const grid = document.getElementById("category-grid");
    if (!grid) return;
    grid.innerHTML = CATEGORIES.map(
      (cat) => `
      <li>
        <a href="${buildListUrl({ filter: "all", category: cat.name, city: "", query: "", sort: "yeni" })}">
          ${iconSvg(cat.icon)}
          <span>${escapeHtml(cat.name)}</span>
          <span class="count">${new Intl.NumberFormat("tr-TR").format(cat.count)}</span>
        </a>
      </li>`
    ).join("");
  };

  const renderCities = () => {
    const grid = document.getElementById("city-grid");
    if (!grid) return;
    grid.innerHTML = POPULAR_CITIES.map(
      (city) => `
      <li>
        <a href="${buildListUrl({ filter: "all", category: "", city: city.name, query: "", sort: "yeni" })}">
          <span>${escapeHtml(city.name)}</span>
          <span class="count">${new Intl.NumberFormat("tr-TR").format(city.count)}</span>
        </a>
      </li>`
    ).join("");
  };

  const fillCitySelect = () => {
    const select = document.getElementById("filter-city");
    if (!select) return;
    ILLER.forEach((city) => {
      const opt = document.createElement("option");
      opt.value = city;
      opt.textContent = city;
      select.appendChild(opt);
    });
  };

  const syncSelectFilled = (el) => {
    if (!el) return;
    el.classList.toggle("is-filled", Boolean(el.value));
  };

  const syncControls = () => {
    const q = document.getElementById("search-query");
    const cat = document.getElementById("filter-category");
    const city = document.getElementById("filter-city");
    if (q) q.value = state.query || "";
    if (cat) cat.value = state.category || "";
    if (city) city.value = state.city || "";
    syncSelectFilled(cat);
    syncSelectFilled(city);
    document.querySelectorAll(".chip, .intent-tab").forEach((el) => {
      el.setAttribute("aria-pressed", el.dataset.filter === state.filter ? "true" : "false");
    });
  };

  fillCitySelect();
  renderCategories();
  renderCities();
  syncControls();
  renderListings();
  renderRecent();

  document.getElementById("filter-category")?.addEventListener("change", (e) => {
    syncSelectFilled(e.currentTarget);
  });
  document.getElementById("filter-city")?.addEventListener("change", (e) => {
    syncSelectFilled(e.currentTarget);
  });

  document.getElementById("search-form")?.addEventListener("submit", (e) => {
    e.preventDefault();
    state.query = document.getElementById("search-query")?.value || "";
    goToList();
  });

  document.getElementById("filter-form")?.addEventListener("submit", (e) => {
    e.preventDefault();
    state.category = document.getElementById("filter-category")?.value || "";
    state.city = document.getElementById("filter-city")?.value || "";
    goToList();
  });

  document.body.addEventListener("click", (e) => {
    const filterEl = e.target.closest(".chip[data-filter], .intent-tab[data-filter]");
    if (filterEl) {
      e.preventDefault();
      state.filter = filterEl.dataset.filter;
      syncControls();
      renderListings();
      return;
    }
    const catBar = e.target.closest(".cat-bar [data-category], .cat-bar [data-filter]");
    if (catBar?.dataset.category) {
      e.preventDefault();
      window.location.href = buildListUrl({
        filter: "all",
        category: catBar.dataset.category,
        city: "",
        query: "",
        sort: "yeni",
      });
      return;
    }
    if (catBar?.dataset.filter) {
      e.preventDefault();
      window.location.href = buildListUrl({
        filter: catBar.dataset.filter,
        category: "",
        city: "",
        query: "",
        sort: "yeni",
      });
    }
  });

  const toggle = document.querySelector(".nav-toggle");
  const mobile = document.getElementById("nav-mobile");
  toggle?.addEventListener("click", () => {
    const open = mobile.classList.toggle("is-open");
    toggle.setAttribute("aria-expanded", open ? "true" : "false");
  });

  const more = document.getElementById("see-all-listings");
  if (more) more.setAttribute("href", buildListUrl(state));
})();
