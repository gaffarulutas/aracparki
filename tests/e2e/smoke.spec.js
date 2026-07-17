import { test, expect } from "@playwright/test";

test("home loads header, hero, and listings", async ({ page }) => {
  await page.goto("./index.html");
  await expect(page.locator(".site-header")).toBeVisible();
  await expect(page.getByRole("heading", { name: "Makineyi işe göre seç" })).toBeVisible();
  await expect(page.locator(".listing-card").first()).toBeVisible({ timeout: 10_000 });
});

test("list page filters and single-link rows", async ({ page }) => {
  await page.goto("./ilanlar.html");
  await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
  const row = page.locator(".classified-row").first();
  await expect(row).toBeVisible({ timeout: 10_000 });
  await expect(row.locator("a")).toHaveCount(1);
});

test("detail page has h1 before specs", async ({ page }) => {
  await page.goto("./ilan.html?id=1");
  const h1 = page.getByRole("heading", { level: 1 });
  await expect(h1).toBeVisible({ timeout: 10_000 });
  await expect(h1).toContainText("Caterpillar");
  await expect(page.getByRole("heading", { name: "İlan Bilgileri" })).toBeVisible();
});

test("mobile nav opens and closes with Escape", async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("./index.html");
  const toggle = page.locator(".nav-toggle");
  await toggle.click();
  await expect(page.locator("#nav-mobile")).toBeVisible();
  await page.keyboard.press("Escape");
  await expect(page.locator("#nav-mobile")).toBeHidden();
});
