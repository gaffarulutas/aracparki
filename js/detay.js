(() => {
  "use strict";

  const {
    escapeHtml,
    escapeAttr,
    formatHours,
    formatTons,
    formatHp,
    badgeLabel,
    getListing,
    buildListUrl,
    showToast,
    priceHtml,
    STORAGE_RECENT,
    loadJson,
    saveJson,
  } = window.AP;

  const params = new URLSearchParams(window.location.search);
  const id = params.get("id");
  const listing = getListing(id);
  const root = document.getElementById("detail-root");

  let recent = loadJson(STORAGE_RECENT, []);

  const trackRecent = (listingId) => {
    const num = Number(listingId);
    recent = [num, ...recent.filter((x) => x !== num)].slice(0, 6);
    saveJson(STORAGE_RECENT, recent);
  };

  if (!listing) {
    root.setAttribute("aria-busy", "false");
    root.innerHTML = `
      <div class="empty-state">
        <p>İlan bulunamadı.</p>
        <a class="btn btn-machine" href="ilanlar.html">İlan listesine dön</a>
      </div>`;
    return;
  }

  trackRecent(listing.id);
  root.setAttribute("aria-busy", "false");
  document.title = `${listing.title} | Araç Parkı`;

  const descMeta = document.querySelector('meta[name="description"]');
  if (descMeta) {
    const desc = `${listing.title} — ${listing.year}, ${listing.hours} saat, ${listing.city}. ${listing.priceUnit ? "Kiralık" : "Satılık"} iş makinesi ilanı.`;
    descMeta.setAttribute("content", desc.replace(/[^\S\n]+/g, " ").trim().slice(0, 300));
  }

  const ogTitle = document.querySelector('meta[property="og:title"]');
  if (ogTitle) ogTitle.setAttribute("content", `${listing.title} | Araç Parkı`);

  const badge = badgeLabel(listing);
  const price = priceHtml(listing);
  const images = listing.images?.length ? listing.images : [listing.image];
  const listBack = escapeAttr(
    buildListUrl({
      filter:
        listing.intent === "kiralik"
          ? "kiralik"
          : listing.intent === "ikinci-el"
            ? "ikinci-el"
            : "satilik",
      category: listing.category,
      city: "",
      query: "",
      sort: "yeni",
    })
  );
  const phoneHref = escapeAttr(listing.phone.replace(/\s/g, ""));

  root.innerHTML = `
    <nav class="breadcrumb" aria-label="Sayfa yolu">
      <a href="index.html">Anasayfa</a>
      <span class="bc-sep">/</span>
      <a href="${listBack}">${escapeHtml(listing.category)}</a>
      <span class="bc-sep">/</span>
      <span>${escapeHtml(listing.title)}</span>
    </nav>

    <div class="detail-layout">
      <div class="detail-left">
        <h1 class="detail-title">${escapeHtml(listing.title)}</h1>

        <div class="detail-gallery">
          <img id="gallery-main" class="gallery-main" src="${escapeAttr(images[0])}" alt="${escapeAttr(listing.title)}" width="900" height="600" />
        </div>

        <section class="detail-card" aria-labelledby="spec-title">
          <h2 class="detail-section-title" id="spec-title">İlan Bilgileri</h2>
          <table class="spec-table">
            <tbody>
              <tr><th scope="row">İlan No</th><td>${escapeHtml(listing.adNo)}</td></tr>
              <tr><th scope="row">İlan Tarihi</th><td>${escapeHtml(listing.listedAt)}</td></tr>
              <tr><th scope="row">Kategori</th><td>${escapeHtml(listing.category)}</td></tr>
              <tr><th scope="row">Tip</th><td><span class="${escapeAttr(badge.className)}">${escapeHtml(badge.text)}</span></td></tr>
              <tr><th scope="row">Model Yılı</th><td>${escapeHtml(listing.year)}</td></tr>
              <tr><th scope="row">Çalışma Saati</th><td>${escapeHtml(formatHours(listing.hours))}</td></tr>
              <tr><th scope="row">Operasyon Ağırlığı</th><td>${escapeHtml(formatTons(listing.tons))}</td></tr>
              <tr><th scope="row">Motor Gücü</th><td>${escapeHtml(formatHp(listing.hp))}</td></tr>
              <tr><th scope="row">Konum</th><td>${escapeHtml(listing.city)} / ${escapeHtml(listing.district)}</td></tr>
            </tbody>
          </table>
        </section>

        <section class="detail-card" aria-labelledby="desc-title">
          <h2 class="detail-section-title" id="desc-title">Açıklama</h2>
          <p class="detail-desc">${escapeHtml(listing.description)}</p>
        </section>
      </div>

      <aside class="detail-right" aria-label="Fiyat ve iletişim">
        <div class="detail-card detail-buybox sticky-box" id="buybox">
          <div class="detail-price">${price}</div>
          <div class="detail-loc">${escapeHtml(listing.city)} / ${escapeHtml(listing.district)}</div>

          <div class="detail-actions">
            <button type="button" class="btn btn-machine btn-block" id="btn-offer">Teklif Al</button>
            <button type="button" class="btn btn-dark btn-block" id="btn-phone">Telefonu Göster</button>
            <a class="btn btn-ghost btn-block" id="btn-call" href="tel:${phoneHref}" hidden>Ara: ${escapeHtml(listing.phone)}</a>
          </div>

          <div class="seller-box">
            <strong>${escapeHtml(listing.sellerName)}</strong>
            <span>${escapeHtml(listing.seller)}${listing.verified ? " · Doğrulanmış" : ""}</span>
          </div>

          <a class="detail-back" href="${listBack}">← Benzer ilanlara dön</a>
        </div>
      </aside>
    </div>

    <div class="mobile-cta" id="mobile-cta" aria-label="Hızlı işlemler">
      <div class="mobile-cta-price">${price}</div>
      <button type="button" class="btn btn-dark btn-sm" id="btn-phone-mobile">Telefon</button>
      <button type="button" class="btn btn-machine btn-sm" id="btn-offer-mobile">Teklif Al</button>
    </div>
  `;

  const revealPhone = () => {
    const call = document.getElementById("btn-call");
    const phoneBtn = document.getElementById("btn-phone");
    if (call) call.hidden = false;
    if (phoneBtn) phoneBtn.hidden = true;
    showToast(`Telefon: ${listing.phone}`);
  };

  document.getElementById("btn-offer")?.addEventListener("click", () => {
    showToast(`${listing.title} için teklif talebi alındı (demo)`);
  });
  document.getElementById("btn-offer-mobile")?.addEventListener("click", () => {
    showToast(`${listing.title} için teklif talebi alındı (demo)`);
  });
  document.getElementById("btn-phone")?.addEventListener("click", revealPhone);
  document.getElementById("btn-phone-mobile")?.addEventListener("click", revealPhone);
})();
