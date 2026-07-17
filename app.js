(() => {
  "use strict";

  const STORAGE_FAV = "ap_favorites";
  const STORAGE_RECENT = "ap_recent";
  const MAX_RECENT = 6;

  const ILLER = [
    "Adana",
    "Adıyaman",
    "Afyonkarahisar",
    "Ağrı",
    "Aksaray",
    "Amasya",
    "Ankara",
    "Antalya",
    "Ardahan",
    "Artvin",
    "Aydın",
    "Balıkesir",
    "Bartın",
    "Batman",
    "Bayburt",
    "Bilecik",
    "Bingöl",
    "Bitlis",
    "Bolu",
    "Burdur",
    "Bursa",
    "Çanakkale",
    "Çankırı",
    "Çorum",
    "Denizli",
    "Diyarbakır",
    "Düzce",
    "Edirne",
    "Elazığ",
    "Erzincan",
    "Erzurum",
    "Eskişehir",
    "Gaziantep",
    "Giresun",
    "Gümüşhane",
    "Hakkari",
    "Hatay",
    "Iğdır",
    "Isparta",
    "İstanbul",
    "İzmir",
    "Kahramanmaraş",
    "Karabük",
    "Karaman",
    "Kars",
    "Kastamonu",
    "Kayseri",
    "Kırıkkale",
    "Kırklareli",
    "Kırşehir",
    "Kilis",
    "Kocaeli",
    "Konya",
    "Kütahya",
    "Malatya",
    "Manisa",
    "Mardin",
    "Mersin",
    "Muğla",
    "Muş",
    "Nevşehir",
    "Niğde",
    "Ordu",
    "Osmaniye",
    "Rize",
    "Sakarya",
    "Samsun",
    "Siirt",
    "Sinop",
    "Sivas",
    "Şanlıurfa",
    "Şırnak",
    "Tekirdağ",
    "Tokat",
    "Trabzon",
    "Tunceli",
    "Uşak",
    "Van",
    "Yalova",
    "Yozgat",
    "Zonguldak"
  ];

  const POPULAR_CITIES = [
    { name: "İstanbul", count: 1840 },
    { name: "Ankara", count: 1120 },
    { name: "İzmir", count: 960 },
    { name: "Bursa", count: 640 },
    { name: "Antalya", count: 520 },
    { name: "Adana", count: 410 },
    { name: "Konya", count: 380 },
    { name: "Gaziantep", count: 350 },
    { name: "Kocaeli", count: 310 },
    { name: "Mersin", count: 280 },
    { name: "Kayseri", count: 240 },
    { name: "Diyarbakır", count: 210 },
  ];

  const CATEGORIES = [
    { name: "Paletli Ekskavatör", count: 2840, icon: "excavator" },
    { name: "Beko Loder", count: 1920, icon: "backhoe" },
    { name: "Lastikli Yükleyici", count: 1480, icon: "loader" },
    { name: "Forklift", count: 1360, icon: "forklift" },
    { name: "Vinç", count: 920, icon: "crane" },
    { name: "Dozer", count: 640, icon: "dozer" },
    { name: "Greyder", count: 410, icon: "grader" },
    { name: "Silindir", count: 380, icon: "roller" },
    { name: "Mini Ekskavatör", count: 870, icon: "mini" },
    { name: "Telehandler", count: 520, icon: "lift" },
    { name: "Beton", count: 290, icon: "concrete" },
    { name: "Kırıcı", count: 240, icon: "crusher" },
  ];

  const LISTINGS = [
    { id: 1, title: "Caterpillar 320D", category: "Paletli Ekskavatör", intent: "ikinci-el", intents: ["satilik", "ikinci-el"], year: 2019, hours: 7200, tons: 22, hp: 162, city: "İstanbul", district: "Tuzla", price: 4850000, priceUnit: null, seller: "Bayi", verified: true, phone: "02165550101", image: "https://images.unsplash.com/photo-1504307651254-35680f356dfd?auto=format&fit=crop&w=900&q=80" },
    { id: 2, title: "Hidromek HMK 102B", category: "Beko Loder", intent: "satilik", intents: ["satilik", "ikinci-el"], year: 2021, hours: 4100, tons: 8.5, hp: 100, city: "Ankara", district: "Sincan", price: 2750000, priceUnit: null, seller: "Sahibi", verified: true, phone: "03125550102", image: "https://images.unsplash.com/photo-1581092160562-40aa08e78837?auto=format&fit=crop&w=900&q=80" },
    { id: 3, title: "Komatsu PC210", category: "Paletli Ekskavatör", intent: "kiralik", intents: ["kiralik"], year: 2020, hours: 5800, tons: 21, hp: 158, city: "İzmir", district: "Aliağa", price: 18500, priceUnit: "gün", seller: "Bayi", verified: true, phone: "02325550103", image: "https://images.unsplash.com/photo-1541888946425-d81bb19240f5?auto=format&fit=crop&w=900&q=80" },
    { id: 4, title: "Volvo L120H", category: "Lastikli Yükleyici", intent: "satilik", intents: ["satilik", "ikinci-el"], year: 2018, hours: 9100, tons: 20, hp: 276, city: "Bursa", district: "Nilüfer", price: 6200000, priceUnit: null, seller: "Bayi", verified: false, phone: "02245550104", image: "https://images.unsplash.com/photo-1504917595217-d4dc5ebe6122?auto=format&fit=crop&w=900&q=80" },
    { id: 5, title: "JCB 3CX", category: "Beko Loder", intent: "kiralik", intents: ["kiralik"], year: 2022, hours: 2100, tons: 8, hp: 92, city: "Antalya", district: "Kepez", price: 9500, priceUnit: "gün", seller: "Sahibi", verified: true, phone: "02425550105", image: "https://images.unsplash.com/photo-1590644365607-1c3d4c1b0f0b?auto=format&fit=crop&w=900&q=80" },
    { id: 6, title: "Toyota 8FD30", category: "Forklift", intent: "ikinci-el", intents: ["satilik", "ikinci-el"], year: 2017, hours: 6400, tons: 3, hp: 54, city: "Kocaeli", district: "Gebze", price: 890000, priceUnit: null, seller: "Bayi", verified: true, phone: "02625550106", image: "https://images.unsplash.com/photo-1566576721346-d4a3b4eaeb55?auto=format&fit=crop&w=900&q=80" },
    { id: 7, title: "Liebherr LTM 1100", category: "Vinç", intent: "kiralik", intents: ["kiralik"], year: 2016, hours: 4200, tons: 100, hp: 367, city: "Gaziantep", district: "Şehitkamil", price: 45000, priceUnit: "gün", seller: "Bayi", verified: true, phone: "03425550107", image: "https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=900&q=80" },
    { id: 8, title: "Caterpillar D6T", category: "Dozer", intent: "satilik", intents: ["satilik", "ikinci-el"], year: 2015, hours: 11200, tons: 23, hp: 215, city: "Konya", district: "Selçuklu", price: 7100000, priceUnit: null, seller: "Sahibi", verified: false, phone: "03325550108", image: "https://images.unsplash.com/photo-1590496793929-36417d311cba?auto=format&fit=crop&w=900&q=80" },
    { id: 9, title: "Case 580ST", category: "Beko Loder", intent: "ikinci-el", intents: ["satilik", "ikinci-el"], year: 2020, hours: 3500, tons: 8.2, hp: 97, city: "Adana", district: "Seyhan", price: 3180000, priceUnit: null, seller: "Bayi", verified: true, phone: "03225550109", image: "https://images.unsplash.com/photo-1621905252507-b35492cc74b4?auto=format&fit=crop&w=900&q=80" },
    { id: 10, title: "Bobcat E50", category: "Mini Ekskavatör", intent: "kiralik", intents: ["kiralik"], year: 2023, hours: 980, tons: 5.2, hp: 55, city: "Mersin", district: "Yenişehir", price: 6200, priceUnit: "gün", seller: "Bayi", verified: true, phone: "03245550110", image: "https://images.unsplash.com/photo-1581092918056-0c4c3acd3789?auto=format&fit=crop&w=900&q=80" },
    { id: 11, title: "Manitou MT 1840", category: "Telehandler", intent: "satilik", intents: ["satilik", "ikinci-el"], year: 2019, hours: 4700, tons: 12, hp: 101, city: "Kayseri", district: "Melikgazi", price: 2450000, priceUnit: null, seller: "Sahibi", verified: true, phone: "03525550111", image: "https://images.unsplash.com/photo-1581092162384-8987c1d64718?auto=format&fit=crop&w=900&q=80" },
    { id: 12, title: "Doosan DX225", category: "Paletli Ekskavatör", intent: "satilik", intents: ["satilik", "ikinci-el"], year: 2021, hours: 3900, tons: 23.5, hp: 173, city: "Diyarbakır", district: "Bağlar", price: 5120000, priceUnit: null, seller: "Bayi", verified: true, phone: "04125550112", image: "https://images.unsplash.com/photo-1504307651254-35680f356dfd?auto=format&fit=crop&w=900&q=70" },
  ];

  const state = {
    filter: "all",
    category: "",
    city: "",
    query: "",
  };

  let favorites = loadJson(STORAGE_FAV, []);
  let recent = loadJson(STORAGE_RECENT, []);
  let writingUrl = false;

  function loadJson(key, fallback) {
    try {
      const raw = localStorage.getItem(key);
      if (!raw) return fallback;
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed : fallback;
    } catch {
      return fallback;
    }
  }

  function saveJson(key, value) {
    try {
      localStorage.setItem(key, JSON.stringify(value));
    } catch {
      /* quota / private mode */
    }
  }

  const escapeHtml = (value) =>
    String(value)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#39;");

  const formatPrice = (n) =>
    new Intl.NumberFormat("tr-TR", {
      style: "currency",
      currency: "TRY",
      maximumFractionDigits: 0,
    }).format(n);

  const formatHours = (n) => new Intl.NumberFormat("tr-TR").format(n) + " saat";
  const formatTons = (n) => String(n).replace(".", ",") + " ton";
  const formatHp = (n) => n + " HP";

  const badgeLabel = (listing) => {
    if (listing.intent === "kiralik") return { text: "Kiralık", className: "badge badge-rent" };
    if (listing.intent === "ikinci-el") return { text: "Satılık · İkinci El", className: "badge badge-used" };
    return { text: "Satılık", className: "badge" };
  };

  const iconSvg = (type) => {
    const common = 'fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"';
    const paths = {
      excavator: `<path d="M3 17h18"/><path d="M6 17V9l4-4h4l3 5h4"/><path d="M10 17v-4h4v4"/>`,
      backhoe: `<path d="M4 17h16"/><path d="M7 17V8h5l3 4h4"/><path d="M8 12h4"/>`,
      loader: `<path d="M3 16h12l3-5h3"/><path d="M5 16V9h8"/><circle cx="7" cy="17" r="2"/><circle cx="15" cy="17" r="2"/>`,
      forklift: `<path d="M4 18V8h6v10"/><path d="M10 12h6l2 4H10"/><path d="M6 8V4h2"/><circle cx="7" cy="19" r="1.5"/><circle cx="15" cy="19" r="1.5"/>`,
      crane: `<path d="M6 20V6l10-3v4"/><path d="M16 7h4"/><path d="M6 20h12"/>`,
      dozer: `<path d="M3 15h14l3-4"/><path d="M5 15V9h10"/><path d="M3 11H1v6h2"/><circle cx="8" cy="17" r="2"/><circle cx="15" cy="17" r="2"/>`,
      grader: `<path d="M2 16h20"/><path d="M4 16V10h8l4 4"/><circle cx="7" cy="17" r="2"/><circle cx="16" cy="17" r="2"/>`,
      roller: `<circle cx="7" cy="15" r="4"/><circle cx="17" cy="15" r="4"/><path d="M11 13h2"/>`,
      mini: `<path d="M5 17h12"/><path d="M7 17V10l3-3h4l2 4"/><path d="M9 17v-3h4v3"/>`,
      lift: `<path d="M5 20V8h6v12"/><path d="M11 10h7v3"/><path d="M7 5v3"/>`,
      concrete: `<path d="M4 18h16"/><path d="M7 18V8l5-3 5 3v10"/><path d="M9 11h6"/>`,
      crusher: `<path d="M4 7h16v4l-3 7H7l-3-7V7z"/><path d="M8 11h8"/>`,
    };
    return `<svg class="cat-ico" viewBox="0 0 24 24" aria-hidden="true" ${common}>${paths[type] || paths.excavator}</svg>`;
  };

  const getListing = (id) => LISTINGS.find((item) => item.id === Number(id));

  const matchesListing = (listing) => {
    const intentOk =
      state.filter === "all" ||
      listing.intents.includes(state.filter) ||
      listing.intent === state.filter;
    const categoryOk = !state.category || listing.category === state.category;
    const cityOk = !state.city || listing.city === state.city;
    const q = state.query.trim().toLowerCase();
    const queryOk =
      !q ||
      listing.title.toLowerCase().includes(q) ||
      listing.category.toLowerCase().includes(q) ||
      listing.city.toLowerCase().includes(q) ||
      String(listing.id) === q;
    return intentOk && categoryOk && cityOk && queryOk;
  };

  const readUrlState = () => {
    const params = new URLSearchParams(window.location.search);
    const tip = params.get("tip") || "all";
    state.filter = ["all", "satilik", "kiralik", "ikinci-el"].includes(tip) ? tip : "all";
    state.category = params.get("kategori") || "";
    state.city = params.get("il") || "";
    state.query = params.get("q") || "";
  };

  const writeUrlState = () => {
    const params = new URLSearchParams();
    if (state.filter && state.filter !== "all") params.set("tip", state.filter);
    if (state.category) params.set("kategori", state.category);
    if (state.city) params.set("il", state.city);
    if (state.query.trim()) params.set("q", state.query.trim());
    const qs = params.toString();
    const next = qs ? `?${qs}${window.location.hash || "#vitrin"}` : `${window.location.pathname}${window.location.hash || ""}`;
    writingUrl = true;
    history.replaceState(null, "", next);
    writingUrl = false;
  };

  const syncControlsFromState = () => {
    const queryEl = document.getElementById("search-query");
    const filterCat = document.getElementById("filter-category");
    const filterCity = document.getElementById("filter-city");
    if (queryEl) queryEl.value = state.query;
    if (filterCat) filterCat.value = state.category;
    if (filterCity) filterCity.value = state.city;

    document.querySelectorAll("[data-filter]").forEach((el) => {
      if (!el.classList.contains("chip") && !el.classList.contains("intent-tab")) return;
      const pressed = el.dataset.filter === state.filter;
      el.setAttribute("aria-pressed", pressed ? "true" : "false");
    });

    const active = document.getElementById("active-filters");
    if (active) {
      const parts = [];
      if (state.filter !== "all") parts.push(`Tip: ${state.filter}`);
      if (state.category) parts.push(state.category);
      if (state.city) parts.push(state.city);
      if (state.query.trim()) parts.push(`“${state.query.trim()}”`);
      if (parts.length) {
        active.hidden = false;
        active.innerHTML = `Aktif filtre: ${escapeHtml(parts.join(" · "))} <button type="button" class="link-clear" id="clear-filters">Temizle</button>`;
      } else {
        active.hidden = true;
        active.textContent = "";
      }
    }
  };

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

  const isFavorite = (id) => favorites.includes(Number(id));

  const toggleFavorite = (id) => {
    const num = Number(id);
    if (favorites.includes(num)) {
      favorites = favorites.filter((item) => item !== num);
      showToast("Favorilerden çıkarıldı");
    } else {
      favorites = [num, ...favorites];
      showToast("Favorilere eklendi");
    }
    saveJson(STORAGE_FAV, favorites);
    updateFavCount();
    renderFavorites();
    renderListings();
  };

  const trackRecent = (id) => {
    const num = Number(id);
    recent = [num, ...recent.filter((item) => item !== num)].slice(0, MAX_RECENT);
    saveJson(STORAGE_RECENT, recent);
    renderRecent();
  };

  const updateFavCount = () => {
    const el = document.getElementById("fav-count");
    if (el) el.textContent = `(${favorites.length})`;
  };

  const cardHtml = (listing, { compact = false } = {}) => {
    const badge = badgeLabel(listing);
    const title = escapeHtml(listing.title);
    const city = escapeHtml(listing.city);
    const district = escapeHtml(listing.district);
    const seller = escapeHtml(listing.seller);
    const image = escapeHtml(listing.image);
    const phone = escapeHtml(listing.phone.replace(/\s/g, ""));
    const unit = listing.priceUnit ? escapeHtml(listing.priceUnit) : "";
    const price = listing.priceUnit
      ? `${formatPrice(listing.price)}<small>/ ${unit}</small>`
      : formatPrice(listing.price);
    const verified = listing.verified
      ? `<span class="seller-tag verified">Doğrulanmış · ${seller}</span>`
      : `<span class="seller-tag">${seller}</span>`;
    const favOn = isFavorite(listing.id);
    const specs = `
      <div class="listing-specs">
        <span title="Çalışma saati">${formatHours(listing.hours)}</span>
        <span title="Operasyon ağırlığı">${formatTons(listing.tons)}</span>
        <span title="Motor gücü">${formatHp(listing.hp)}</span>
      </div>`;

    return `
      <article class="listing-card${compact ? " is-compact" : ""} is-visible" data-id="${listing.id}">
        <div class="listing-media">
          <span class="${escapeHtml(badge.className)}">${escapeHtml(badge.text)}</span>
          <button type="button" class="fav-btn${favOn ? " is-on" : ""}" data-fav="${listing.id}" aria-label="${favOn ? "Favoriden çıkar" : "Favoriye ekle"}" aria-pressed="${favOn}">♥</button>
          <img src="${image}" alt="${title}" loading="lazy" decoding="async" width="900" height="620" sizes="(max-width:720px) 40vw, 220px" />
        </div>
        <div class="listing-body">
          <h3>${title}</h3>
          <div class="listing-meta">
            <span>${listing.year}</span>
            <span>${city} / ${district}</span>
          </div>
          ${specs}
          <div class="listing-foot">
            <div class="listing-price">${price}</div>
            ${verified}
          </div>
          <div class="listing-cta">
            <button type="button" class="btn btn-machine btn-sm" data-offer="${listing.id}">Teklif Al</button>
            <a class="btn btn-ghost btn-sm" href="tel:${phone}" data-call="${listing.id}">Ara</a>
          </div>
        </div>
      </article>`;
  };

  let observer;
  const observeCards = () => {
    const cards = document.querySelectorAll(".listing-card:not(.is-visible)");
    if (!cards.length) return;
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
      cards.forEach((card) => card.classList.add("is-visible"));
      return;
    }
    if (observer) observer.disconnect();
    observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.12, rootMargin: "0px 0px -30px 0px" }
    );
    cards.forEach((card) => observer.observe(card));
  };

  const renderListings = () => {
    const grid = document.getElementById("listing-grid");
    const countEl = document.getElementById("result-count");
    if (!grid) return;
    const items = LISTINGS.filter(matchesListing);
    if (countEl) countEl.textContent = `(${items.length})`;
    if (!items.length) {
      grid.innerHTML = '<p class="listing-empty">Bu kriterlere uygun ilan yok. Filtreleri temizlemeyi dene.</p>';
      return;
    }
    grid.innerHTML = items.map((item) => cardHtml(item)).join("");
    requestAnimationFrame(() => {
      grid.querySelectorAll(".listing-card").forEach((card) => card.classList.remove("is-visible"));
      observeCards();
    });
  };

  const renderRecent = () => {
    const section = document.getElementById("recent");
    const grid = document.getElementById("recent-grid");
    if (!section || !grid) return;
    const items = recent.map(getListing).filter(Boolean);
    if (!items.length) {
      section.hidden = true;
      grid.innerHTML = "";
      return;
    }
    section.hidden = false;
    grid.innerHTML = items.map((item) => cardHtml(item, { compact: true })).join("");
  };

  const renderFavorites = () => {
    const list = document.getElementById("fav-list");
    if (!list) return;
    const items = favorites.map(getListing).filter(Boolean);
    if (!items.length) {
      list.innerHTML = '<p class="listing-empty">Henüz favori ilan yok.</p>';
      return;
    }
    list.innerHTML = items.map((item) => cardHtml(item, { compact: true })).join("");
  };

  const renderCategories = () => {
    const grid = document.getElementById("category-grid");
    if (!grid) return;
    grid.innerHTML = CATEGORIES.map(
      (cat) => `
      <li>
        <a href="#vitrin" data-category="${escapeHtml(cat.name)}">
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
        <a href="#vitrin" data-city="${escapeHtml(city.name)}">
          <span>${escapeHtml(city.name)}</span>
          <span class="count">${new Intl.NumberFormat("tr-TR").format(city.count)}</span>
        </a>
      </li>`
    ).join("");
  };

  const fillCitySelect = () => {
    const select = document.getElementById("filter-city");
    if (!select) return;
    const fragment = document.createDocumentFragment();
    ILLER.forEach((city) => {
      const opt = document.createElement("option");
      opt.value = city;
      opt.textContent = city;
      fragment.appendChild(opt);
    });
    select.appendChild(fragment);
  };

  const applyState = ({ scroll = false, pushUrl = true } = {}) => {
    syncControlsFromState();
    if (pushUrl) writeUrlState();
    renderListings();
    if (scroll) {
      document.getElementById("vitrin")?.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  };

  const setFilter = (filter, opts) => {
    state.filter = filter || "all";
    applyState(opts);
  };

  const setCategory = (category, opts) => {
    state.category = category || "";
    state.filter = "all";
    applyState(opts);
  };

  const setCity = (city, opts) => {
    state.city = city || "";
    applyState(opts);
  };

  const clearAllFilters = () => {
    state.filter = "all";
    state.category = "";
    state.city = "";
    state.query = "";
    applyState({ scroll: true });
  };

  const bindEvents = () => {
    document.getElementById("search-form")?.addEventListener("submit", (e) => {
      e.preventDefault();
      state.query = document.getElementById("search-query")?.value || "";
      applyState({ scroll: true });
    });

    document.getElementById("filter-form")?.addEventListener("submit", (e) => {
      e.preventDefault();
      state.category = document.getElementById("filter-category")?.value || "";
      state.city = document.getElementById("filter-city")?.value || "";
      applyState({ scroll: true });
    });

    document.body.addEventListener("click", (e) => {
      const clearBtn = e.target.closest("#clear-filters");
      if (clearBtn) {
        clearAllFilters();
        return;
      }

      const favBtn = e.target.closest("[data-fav]");
      if (favBtn) {
        e.preventDefault();
        toggleFavorite(favBtn.dataset.fav);
        return;
      }

      const offerBtn = e.target.closest("[data-offer]");
      if (offerBtn) {
        e.preventDefault();
        const listing = getListing(offerBtn.dataset.offer);
        if (!listing) return;
        trackRecent(listing.id);
        showToast(`${listing.title} için teklif talebi alındı (demo)`);
        return;
      }

      const callBtn = e.target.closest("[data-call]");
      if (callBtn) {
        const listing = getListing(callBtn.dataset.call);
        if (listing) trackRecent(listing.id);
        return;
      }

      const filterEl = e.target.closest("[data-filter]");
      if (filterEl && (filterEl.classList.contains("chip") || filterEl.classList.contains("intent-tab") || filterEl.closest(".cat-bar") || filterEl.closest(".nav-mobile"))) {
        e.preventDefault();
        setFilter(filterEl.dataset.filter, { scroll: true });
        return;
      }

      const catLink = e.target.closest("[data-category]");
      if (catLink) {
        e.preventDefault();
        setCategory(catLink.dataset.category, { scroll: true });
        return;
      }

      const cityLink = e.target.closest("[data-city]");
      if (cityLink) {
        e.preventDefault();
        setCity(cityLink.dataset.city, { scroll: true });
      }
    });

    const toggle = document.querySelector(".nav-toggle");
    const mobile = document.getElementById("nav-mobile");
    toggle?.addEventListener("click", () => {
      const open = mobile.classList.toggle("is-open");
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
      toggle.setAttribute("aria-label", open ? "Menüyü kapat" : "Menüyü aç");
    });
    mobile?.querySelectorAll("a").forEach((a) => {
      a.addEventListener("click", () => {
        mobile.classList.remove("is-open");
        toggle?.setAttribute("aria-expanded", "false");
      });
    });

    const favPanel = document.getElementById("fav-panel");
    const favBtn = document.getElementById("fav-panel-btn");
    const favClose = document.getElementById("fav-panel-close");
    const openFav = (open) => {
      if (!favPanel || !favBtn) return;
      favPanel.hidden = !open;
      favBtn.setAttribute("aria-expanded", open ? "true" : "false");
      if (open) renderFavorites();
    };
    favBtn?.addEventListener("click", () => openFav(favPanel.hidden));
    favClose?.addEventListener("click", () => openFav(false));

    window.addEventListener("popstate", () => {
      if (writingUrl) return;
      readUrlState();
      applyState({ pushUrl: false });
    });
  };

  readUrlState();
  fillCitySelect();
  renderCategories();
  renderCities();
  updateFavCount();
  renderRecent();
  applyState({ pushUrl: true });
  bindEvents();
})();
