import { test, expect, Route } from '@playwright/test';

test.describe('Launch New Gig Form', () => {
  test.beforeEach(async ({ page }) => {
    const postedBodies: Array<{ title: string; assignee: string }> = [];

    await page.route('**/to-do**', async (route: Route) => {
      const request = route.request();
      const method = request.method();

      if (method === 'OPTIONS') {
        await route.fulfill({
          status: 204,
          headers: {
            'access-control-allow-origin': '*',
            'access-control-allow-methods': 'GET,POST,PATCH,DELETE,OPTIONS',
            'access-control-allow-headers': 'content-type',
          },
        });
        return;
      }

      if (method === 'POST') {
        const payload = await request.postDataJSON();
        postedBodies.push(payload);
        await new Promise(resolve => setTimeout(resolve, 25));
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 90210,
            isCompleted: false,
            ...payload,
          }),
        });
        return;
      }

      await route.continue();
    });

    await page.goto('/list/add');
  });

  test('keeps deploy disabled until form valid', async ({ page }) => {
    const deployButton = page.getByRole('button', { name: 'Deploy gig' });
    const titleInput = page.locator('#title');
    const assigneeInput = page.locator('#assignee');

    await expect(deployButton).toBeDisabled();

    await titleInput.fill('Night market oversight');
    await expect(deployButton).toBeDisabled();

    await assigneeInput.fill('Rogue');
    await expect(deployButton).toBeEnabled();
  });

  test('submits mission payload to the API', async ({ page }) => {
    const titleInput = page.locator('#title');
    const assigneeInput = page.locator('#assignee');
    const deployButton = page.getByRole('button', { name: 'Deploy gig' });

    await titleInput.fill('Secure Delamain shard');
    await assigneeInput.fill('Claire Russell');

    const requestPromise = page.waitForRequest(req => req.method() === 'POST' && req.url().includes('/to-do'));
    await deployButton.click();

    const postRequest = await requestPromise;
    expect(await postRequest.postDataJSON()).toEqual({
      title: 'Secure Delamain shard',
      assignee: 'Claire Russell',
    });
  });

  test('clears the form back to pristine state', async ({ page }) => {
    const titleInput = page.locator('#title');
    const assigneeInput = page.locator('#assignee');
    const deployButton = page.getByRole('button', { name: 'Deploy gig' });
    const clearButton = page.getByRole('button', { name: 'Clear' });

    await titleInput.fill('Ghostwire relay');
    await assigneeInput.fill('River Ward');
    await expect(deployButton).toBeEnabled();

    await clearButton.click();

    await expect(titleInput).toHaveValue('');
    await expect(assigneeInput).toHaveValue('');
    await expect(deployButton).toBeDisabled();
  });
});

