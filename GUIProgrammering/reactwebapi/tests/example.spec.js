import { test, expect } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://localhost:5173';

test('has title', async ({ page }) => {
  await page.goto(BASE_URL);
  await expect(page).toHaveTitle(/Aura Edit/);
});

test('get started link', async ({ page }) => {
  await page.goto(BASE_URL);
  await page.locator('#root > div > main > header > a').click();
  await expect(page.getByRole('heading', { name: 'Featured products' })).toBeVisible();
});


test('layout har header, navigation, main og footer', async ({ page }) => {
  await page.goto(BASE_URL);
  await expect(page.locator('#root > div > main > header')).toBeVisible();
  await expect(page.locator('#root > div > main > nav')).toBeVisible();
  await expect(page.locator('#root > div > main > div')).toBeVisible();
  await expect(page.locator('#root > div > footer')).toBeVisible();
});

test('kan navigere til Products', async ({ page }) => {
  await page.goto(BASE_URL);

  const productsLink = page.getByRole('link', { name: /all products/i });
  if (await productsLink.count()) {
    await Promise.all([
      page.waitForURL('**/products*'),
      productsLink.first().click(),
    ]);
  } else {
    await page.goto(`${BASE_URL}/products`);
  }

  await expect(page).toHaveURL(/\/products(?:\/)?(?:\?.*)?$/);

  await expect(page.getByRole('heading', { name: /(all )?products/i })).toBeVisible();
});

test('kan navigere til Cart', async ({ page }) => {
  await page.goto(BASE_URL);
  const cartLink = page.getByRole('link', { name: /(cart|basket|kurv)/i });
  if (await cartLink.count()) {
    await cartLink.first().click();
  } else {
    await page.goto(`${BASE_URL}/cart`);
  }
  await expect(page).toHaveURL(/\/cart\/?$/);
  await expect(page.getByRole('heading', { name: /(cart|basket|kurv|your cart|din kurv)/i })).toBeVisible();
});

test('featured products sektionen indeholder indhold', async ({ page }) => {
  await page.goto(BASE_URL);
  const cta = page.locator('#root > div > main > header > a');
  if (await cta.count()) {
    await cta.first().click();
  }

  const heading = page.getByRole('heading', { name: /featured products/i });
  await expect(heading).toBeVisible();

  const container = heading.locator('xpath=following::*[self::section or self::div][1]');
  await expect(container.locator('a, img').first()).toBeVisible();

  const count = await container.locator('a, img').count();
  expect(count).toBeGreaterThan(0);
});

test('ingen console errors på forsiden', async ({ page }) => {
  const errors = [];
  page.on('console', msg => {
    if (msg.type() === 'error') errors.push(msg.text());
  });
  await page.goto(BASE_URL);
  expect(errors, `Console errors: ${errors.join('\n')}`).toHaveLength(0);
});

test('footer er synlig på flere routes', async ({ page }) => {
  for (const route of ['/', '/products', '/cart']) {
    await page.goto(`${BASE_URL}${route}`);
    await expect(page.getByRole('contentinfo')).toBeVisible();
  }
});