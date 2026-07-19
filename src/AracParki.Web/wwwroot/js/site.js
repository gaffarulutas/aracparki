(() => {
  "use strict";

  const STORAGE_RECENT = "ap:recent";

  const toast = (message) => {
    const el = document.getElementById("toast");
    if (!el || !message) return;
    el.hidden = false;
    el.textContent = String(message);
    clearTimeout(toast._t);
    toast._t = setTimeout(() => {
      el.hidden = true;
      el.textContent = "";
    }, 2800);
  };

  const trackRecent = () => {
    const host = document.querySelector("[data-recent-id]");
    if (!host) return;
    const id = Number(host.getAttribute("data-recent-id"));
    if (!Number.isFinite(id)) return;
    let recent = [];
    try {
      recent = JSON.parse(localStorage.getItem(STORAGE_RECENT) || "[]");
    } catch {
      recent = [];
    }
    recent = [id, ...recent.filter((x) => x !== id)].slice(0, 6);
    localStorage.setItem(STORAGE_RECENT, JSON.stringify(recent));
  };

  document.addEventListener("click", (e) => {
    const toastBtn = e.target.closest("[data-toast]");
    if (toastBtn) {
      toast(toastBtn.getAttribute("data-toast"));
    }

    const sellerBtn = e.target.closest("[data-seller-cta]");
    if (sellerBtn) {
      toast("İlan verme yakında açılacak");
    }
  });

  document.addEventListener("click", async (e) => {
    const btn = e.target.closest("[data-reveal-phone]");
    if (!btn) return;
    e.preventDefault();
    const adNo = btn.getAttribute("data-reveal-phone");
    if (!adNo) return;
    btn.disabled = true;
    try {
      const res = await fetch(`/ilan/${encodeURIComponent(adNo)}/telefon`, {
        method: "POST",
        headers: { Accept: "application/json" },
      });
      if (res.status === 429) {
        toast("Çok fazla deneme. Bir dakika sonra tekrar deneyin.");
        return;
      }
      if (!res.ok) {
        toast("Telefon alınamadı.");
        return;
      }
      const data = await res.json();
      const phone = data?.phone;
      if (!phone) {
        toast("Telefon alınamadı.");
        return;
      }
      toast(`Telefon: ${phone}`);
      document.querySelectorAll("[data-phone-slot]").forEach((slot) => {
        slot.hidden = false;
        const link = slot.querySelector("a[data-phone-link]");
        if (link) {
          link.href = `tel:${String(phone).replace(/\s/g, "")}`;
          link.textContent = `Ara: ${phone}`;
        }
      });
      document.querySelectorAll("[data-reveal-phone]").forEach((el) => {
        el.hidden = true;
      });
    } catch {
      toast("Telefon alınamadı.");
    } finally {
      btn.disabled = false;
    }
  });

  document.addEventListener("alpine:init", () => {
    Alpine.data("siteChrome", () => ({
      navOpen: false,
      searchOpen: false,
      syncAria() {
        const nav = this.$refs.navToggle;
        if (nav) {
          nav.setAttribute("aria-expanded", String(this.navOpen));
          nav.setAttribute("aria-label", this.navOpen ? "Menüyü kapat" : "Menüyü aç");
        }
        const search = this.$refs.searchToggle;
        if (search) {
          search.setAttribute("aria-expanded", String(this.searchOpen));
          search.setAttribute("aria-label", this.searchOpen ? "Aramayı kapat" : "Aramayı aç");
        }
      },
      toggleNav() {
        this.navOpen = !this.navOpen;
        this.searchOpen = false;
        document.querySelector(".site-header")?.classList.remove("is-search-open");
        this.syncAria();
        if (this.navOpen) {
          this.$nextTick(() => document.querySelector("#nav-mobile a")?.focus());
        }
      },
      closeNav() {
        this.navOpen = false;
        this.syncAria();
      },
      toggleSearch() {
        this.searchOpen = !this.searchOpen;
        this.navOpen = false;
        document.querySelector(".site-header")?.classList.toggle("is-search-open", this.searchOpen);
        this.syncAria();
        if (this.searchOpen) {
          this.$nextTick(() => document.getElementById("search-query")?.focus());
        }
      },
      init() {
        window.addEventListener("keydown", (e) => {
          if (e.key !== "Escape") return;
          this.navOpen = false;
          this.searchOpen = false;
          document.querySelector(".site-header")?.classList.remove("is-search-open");
          this.syncAria();
        });
      },
    }));

    Alpine.data("authForm", () => ({
      showPassword: false,
      password: "",
      get passwordType() {
        return this.showPassword ? "text" : "password";
      },
      get passwordToggleLabel() {
        return this.showPassword ? "Şifreyi gizle" : "Şifreyi göster";
      },
      get showEye() {
        return !this.showPassword;
      },
      get showEyeOff() {
        return this.showPassword;
      },
      togglePassword() {
        this.showPassword = !this.showPassword;
      },
      ruleClass(key) {
        const p = this.password || "";
        const ok =
          key === "len"
            ? p.length >= 8
            : key === "letter"
              ? /[A-Za-zÀ-ÿ]/.test(p)
              : key === "digit"
                ? /\d/.test(p)
                : key === "noTriple"
                  ? p.length > 0 && !/(.)\1\1/.test(p)
                  : false;
        return ok ? "is-ok" : "";
      },
    }));
  });

  document.addEventListener("DOMContentLoaded", () => {
    trackRecent();
    const boot = document.getElementById("toast")?.getAttribute("data-boot-toast");
    if (boot) toast(boot);
  });

  window.AP = { toast, STORAGE_RECENT };
})();
