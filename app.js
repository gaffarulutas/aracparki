(() => {
  "use strict";

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
    "Zonguldak",
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
    {
      id: 1,
      title: "Caterpillar 320D",
      category: "Paletli Ekskavatör",
      intent: "ikinci-el",
      intents: ["satilik", "ikinci-el"],
      year: 2019,
      hours: 7200,
      city: "İstanbul",
      district: "Tuzla",
      price: 4850000,
      priceUnit: null,
      seller: "Bayi",
      verified: true,
      image: "https://images.unsplash.com/photo-1504307651254-35680f356dfd?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 2,
      title: "Hidromek HMK 102B",
      category: "Beko Loder",
      intent: "satilik",
      intents: ["satilik", "ikinci-el"],
      year: 2021,
      hours: 4100,
      city: "Ankara",
      district: "Sincan",
      price: 2750000,
      priceUnit: null,
      seller: "Sahibi",
      verified: true,
      image: "https://images.unsplash.com/photo-1581092160562-40aa08e78837?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 3,
      title: "Komatsu PC210",
      category: "Paletli Ekskavatör",
      intent: "kiralik",
      intents: ["kiralik"],
      year: 2020,
      hours: 5800,
      city: "İzmir",
      district: "Aliağa",
      price: 18500,
      priceUnit: "gün",
      seller: "Bayi",
      verified: true,
      image: "https://images.unsplash.com/photo-1541888946425-d81bb19240f5?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 4,
      title: "Volvo L120H",
      category: "Lastikli Yükleyici",
      intent: "satilik",
      intents: ["satilik", "ikinci-el"],
      year: 2018,
      hours: 9100,
      city: "Bursa",
      district: "Nilüfer",
      price: 6200000,
      priceUnit: null,
      seller: "Bayi",
      verified: false,
      image: "https://images.unsplash.com/photo-1504917595217-d4dc5ebe6122?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 5,
      title: "JCB 3CX",
      category: "Beko Loder",
      intent: "kiralik",
      intents: ["kiralik"],
      year: 2022,
      hours: 2100,
      city: "Antalya",
      district: "Kepez",
      price: 9500,
      priceUnit: "gün",
      seller: "Sahibi",
      verified: true,
      image: "https://images.unsplash.com/photo-1590644365607-1c3d4c1b0f0b?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 6,
      title: "Toyota 8FD30",
      category: "Forklift",
      intent: "ikinci-el",
      intents: ["satilik", "ikinci-el"],
      year: 2017,
      hours: 6400,
      city: "Kocaeli",
      district: "Gebze",
      price: 890000,
      priceUnit: null,
      seller: "Bayi",
      verified: true,
      image: "https://images.unsplash.com/photo-1566576721346-d4a3b4eaeb55?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 7,
      title: "Liebherr LTM 1100",
      category: "Vinç",
      intent: "kiralik",
      intents: ["kiralik"],
      year: 2016,
      hours: 4200,
      city: "Gaziantep",
      district: "Şehitkamil",
      price: 45000,
      priceUnit: "gün",
      seller: "Bayi",
      verified: true,
      image: "https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 8,
      title: "Caterpillar D6T",
      category: "Dozer",
      intent: "satilik",
      intents: ["satilik", "ikinci-el"],
      year: 2015,
      hours: 11200,
      city: "Konya",
      district: "Selçuklu",
      price: 7100000,
      priceUnit: null,
      seller: "Sahibi",
      verified: false,
      image: "https://images.unsplash.com/photo-1590496793929-36417d311cba?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 9,
      title: "Case 580ST",
      category: "Beko Loder",
      intent: "ikinci-el",
      intents: ["satilik", "ikinci-el"],
      year: 2020,
      hours: 3500,
      city: "Adana",
      district: "Seyhan",
      price: 3180000,
      priceUnit: null,
      seller: "Bayi",
      verified: true,
      image: "https://images.unsplash.com/photo-1621905252507-b35492cc74b4?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 10,
      title: "Bobcat E50",
      category: "Mini Ekskavatör",
      intent: "kiralik",
      intents: ["kiralik"],
      year: 2023,
      hours: 980,
      city: "Mersin",
      district: "Yenişehir",
      price: 6200,
      priceUnit: "gün",
      seller: "Bayi",
      verified: true,
      image: "https://images.unsplash.com/photo-1581092918056-0c4c3acd3789?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 11,
      title: "Manitou MT 1840",
      category: "Telehandler",
      intent: "satilik",
      intents: ["satilik", "ikinci-el"],
      year: 2019,
      hours: 4700,
      city: "Kayseri",
      district: "Melikgazi",
      price: 2450000,
      priceUnit: null,
      seller: "Sahibi",
      verified: true,
      image: "https://images.unsplash.com/photo-1581092162384-8987c1d64718?auto=format&fit=crop&w=900&q=80",
    },
    {
      id: 12,
      title: "Doosan DX225",
      category: "Paletli Ekskavatör",
      intent: "satilik",
      intents: ["satilik", "ikinci-el"],
      year: 2021,
      hours: 3900,
      city: "Diyarbakır",
      district: "Bağlar",
      price: 5120000,
      priceUnit: null,
      seller: "Bayi",
      verified: true,
      image: "https://images.unsplash.com/photo-1504307651254-35680f356dfd?auto=format&fit=crop&w=900&q=70",
    },
  ];

  const state = {
    intent: "satilik",
    filter: "all",
    category: "",
    city: "",
    query: "",
  };


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

  const formatHours = (n) =>
    new Intl.NumberFormat("tr-TR").format(n) + " saat";

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

  const matchesListing = (listing) => {
    const intentOk =
      state.filter === "all" ||
      listing.intents.includes(state.filter) ||
      listing.intent === state.filter;

    const categoryOk =
      !state.category || listing.category === state.category;

    const cityOk = !state.city || listing.city === state.city;

    const q = state.query.trim().toLowerCase();
    const queryOk =
      !q ||
      listing.title.toLowerCase().includes(q) ||
      listing.category.toLowerCase().includes(q) ||
      listing.city.toLowerCase().includes(q);

    return intentOk && categoryOk && cityOk && queryOk;
  };

  const renderListings = () => {
    const grid = document.getElementById("listing-grid");
    if (!grid) return;

    const items = LISTINGS.filter(matchesListing);
    if (!items.length) {
      grid.innerHTML =
        '<p class="listing-empty">Bu kriterlere uygun ilan yok. Filtreleri genişletmeyi dene.</p>';
      return;
    }

    grid.innerHTML = items
      .map((listing, index) => {
        const badge = badgeLabel(listing);
        const title = escapeHtml(listing.title);
        const city = escapeHtml(listing.city);
        const district = escapeHtml(listing.district);
        const seller = escapeHtml(listing.seller);
        const image = escapeHtml(listing.image);
        const unit = listing.priceUnit ? escapeHtml(listing.priceUnit) : "";
        const price = listing.priceUnit
          ? `${formatPrice(listing.price)}<small>/ ${unit}</small>`
          : formatPrice(listing.price);
        const verified = listing.verified
          ? `<span class="seller-tag verified">Doğrulanmış · ${seller}</span>`
          : `<span class="seller-tag">${seller}</span>`;

        return `
          <article class="listing-card" data-index="${index}" style="transition-delay:${index * 45}ms">
            <a class="listing-link" href="#vitrin" aria-label="${title} ilanını incele">
              <div class="listing-media">
                <span class="${escapeHtml(badge.className)}">${escapeHtml(badge.text)}</span>
                <img src="${image}" alt="${title}" loading="lazy" decoding="async" width="900" height="620" />
              </div>
              <div class="listing-body">
                <h3>${title}</h3>
                <div class="listing-meta">
                  <span>${listing.year} · ${formatHours(listing.hours)}</span>
                  <span>${city} / ${district}</span>
                </div>
                <div class="listing-foot">
                  <div class="listing-price">${price}</div>
                  ${verified}
                </div>
              </div>
            </a>
          </article>
        `;
      })
      .join("");

    requestAnimationFrame(() => observeCards());
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

  const syncHiddenSearch = () => {
    const cat = document.getElementById("search-category");
    const city = document.getElementById("search-city");
    if (cat) cat.value = state.category;
    if (city) city.value = state.city;
  };

  const setIntentTabs = (intent) => {
    state.intent = intent;
    document.getElementById("search-intent").value = intent;
    document.querySelectorAll(".intent-tab").forEach((tab) => {
      const selected = tab.dataset.intent === intent;
      tab.setAttribute("aria-pressed", selected ? "true" : "false");
    });
  };

  const setFilterChips = (filter) => {
    state.filter = filter;
    document.querySelectorAll(".chip").forEach((chip) => {
      const pressed = chip.dataset.filter === filter;
      chip.setAttribute("aria-pressed", pressed ? "true" : "false");
    });
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
      { threshold: 0.15, rootMargin: "0px 0px -40px 0px" }
    );
    cards.forEach((card) => observer.observe(card));
  };

  const applySearchFromForm = ({ syncFilters = false } = {}) => {
    const filterCat = document.getElementById("filter-category");
    const filterCity = document.getElementById("filter-city");
    const queryEl = document.getElementById("search-query");
    const mobileQuery = document.getElementById("filter-query-mobile");
    if (syncFilters) {
      if (filterCat) state.category = filterCat.value;
      if (filterCity) state.city = filterCity.value;
    }
    const headerQ = queryEl?.value?.trim() || "";
    const mobileQ = mobileQuery?.value?.trim() || "";
    state.query = headerQ || mobileQ;
    if (queryEl && state.query) queryEl.value = state.query;
    if (mobileQuery && state.query) mobileQuery.value = state.query;
    syncHiddenSearch();
    renderListings();
    document.getElementById("vitrin")?.scrollIntoView({ behavior: "smooth", block: "start" });
  };

  const clearFilterControls = () => {
    const filterCat = document.getElementById("filter-category");
    const filterCity = document.getElementById("filter-city");
    const queryEl = document.getElementById("search-query");
    const mobileQuery = document.getElementById("filter-query-mobile");
    if (filterCat) filterCat.value = "";
    if (filterCity) filterCity.value = "";
    if (queryEl) queryEl.value = "";
    if (mobileQuery) mobileQuery.value = "";
    syncHiddenSearch();
  };

  const setCategoryFilter = (category) => {
    state.category = category;
    state.city = "";
    state.query = "";
    state.filter = "all";
    setFilterChips("all");
    const filterCat = document.getElementById("filter-category");
    const filterCity = document.getElementById("filter-city");
    const queryEl = document.getElementById("search-query");
    if (filterCat) filterCat.value = category;
    if (filterCity) filterCity.value = "";
    if (queryEl) queryEl.value = "";
    syncHiddenSearch();
    renderListings();
    document.getElementById("vitrin")?.scrollIntoView({ behavior: "smooth" });
  };

  const setCityFilter = (city) => {
    state.city = city;
    state.category = "";
    state.query = "";
    state.filter = "all";
    setFilterChips("all");
    const filterCat = document.getElementById("filter-category");
    const filterCity = document.getElementById("filter-city");
    const queryEl = document.getElementById("search-query");
    if (filterCat) filterCat.value = "";
    if (filterCity) filterCity.value = city;
    if (queryEl) queryEl.value = "";
    syncHiddenSearch();
    renderListings();
    document.getElementById("vitrin")?.scrollIntoView({ behavior: "smooth" });
  };

  const bindEvents = () => {
    document.querySelectorAll(".intent-tab").forEach((tab) => {
      tab.addEventListener("click", () => {
        setIntentTabs(tab.dataset.intent);
        setFilterChips(tab.dataset.intent);
        renderListings();
        document.getElementById("vitrin")?.scrollIntoView({ behavior: "smooth" });
      });
    });

    document.getElementById("search-form")?.addEventListener("submit", (e) => {
      e.preventDefault();
      applySearchFromForm({ syncFilters: false });
    });

    document.getElementById("filter-apply")?.addEventListener("click", () => {
      state.filter = "all";
      setFilterChips("all");
      applySearchFromForm({ syncFilters: true });
    });

    document.querySelectorAll(".chip").forEach((chip) => {
      chip.addEventListener("click", () => {
        setFilterChips(chip.dataset.filter);
        state.category = "";
        state.city = "";
        state.query = "";
        clearFilterControls();
        renderListings();
      });
    });

    document.getElementById("category-grid")?.addEventListener("click", (e) => {
      const link = e.target.closest("[data-category]");
      if (!link) return;
      e.preventDefault();
      setCategoryFilter(link.dataset.category);
    });

    document.querySelector(".cat-bar")?.addEventListener("click", (e) => {
      const link = e.target.closest("[data-category]");
      if (!link) return;
      e.preventDefault();
      setCategoryFilter(link.dataset.category);
    });

    document.getElementById("city-grid")?.addEventListener("click", (e) => {
      const link = e.target.closest("[data-city]");
      if (!link) return;
      e.preventDefault();
      setCityFilter(link.dataset.city);
    });

    document.querySelectorAll("[data-intent]").forEach((el) => {
      if (el.classList.contains("intent-tab")) return;
      el.addEventListener("click", () => {
        const intent = el.dataset.intent;
        if (!intent) return;
        setIntentTabs(intent);
        setFilterChips(intent);
        state.category = "";
        state.city = "";
        state.query = "";
        clearFilterControls();
        renderListings();
      });
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
        toggle?.setAttribute("aria-label", "Menüyü aç");
      });
    });
  };

  fillCitySelect();
  renderCategories();
  renderCities();
  renderListings();
  bindEvents();
})();
