import { test, expect, Page, Route } from '@playwright/test';

type ToDo = {
  id: number;
  title: string;
  assignee: string;
  isCompleted?: boolean;
};

const baseTodos: ToDo[] = [
  { id: 1, title: 'Breach Arasaka ICE', assignee: 'V' },
  { id: 2, title: 'Extract Militech intel', assignee: 'Panam' },
  { id: 3, title: 'Broker Tyger Claws truce', assignee: 'Judy' },
];

async function primeTodoApi(page: Page, seed: ToDo[]) {
  const todos = seed.map(todo => ({ ...todo }));
  let nextId = Math.max(...todos.map(todo => todo.id)) + 1;

  await page.route('**/to-do**', async (route: Route) => {
    const request = route.request();
    const method = request.method();
    const url = new URL(request.url());
    const path = url.pathname;

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

    if (method === 'GET' && path.endsWith('/to-do')) {
      await new Promise(resolve => setTimeout(resolve, 20));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(todos),
      });
      return;
    }

    if (method === 'POST' && path.endsWith('/to-do')) {
      const payload = await request.postDataJSON();
      const created = {
        id: nextId++,
        isCompleted: false,
        ...payload,
      };
      todos.push(created);
      await new Promise(resolve => setTimeout(resolve, 20));
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify(created),
      });
      return;
    }

    const match = path.match(/\/to-do\/(\d+)$/);
    if (match) {
      const id = Number(match[1]);
      const index = todos.findIndex(todo => todo.id === id);

      if (method === 'PATCH') {
        const payload = await request.postDataJSON();
        if (index >= 0) {
          todos[index] = { ...todos[index], ...payload };
        }
        await new Promise(resolve => setTimeout(resolve, 30));
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(todos[index]),
        });
        return;
      }

      if (method === 'DELETE') {
        if (index >= 0) {
          todos.splice(index, 1);
        }
        await new Promise(resolve => setTimeout(resolve, 30));
        await route.fulfill({
          status: 204,
          body: '',
        });
        return;
      }
    }

    await route.continue();
  });

  return todos;
}

test.describe('Mission Board', () => {
  test.beforeEach(async ({ page }) => {
    await primeTodoApi(page, baseTodos);
    await page.goto('/list');
    await page.waitForResponse(resp => resp.url().includes('/to-do') && resp.request().method() === 'GET');
  });

  test('displays mission board overview', async ({ page }) => {
    await expect(page.locator('h2')).toHaveText('Mission Board');
    const link = page.locator('a.add-link');
    await expect(link).toHaveText('Launch new gig');
    await expect(link).toHaveAttribute('href', '/list/add');
  });

  test('renders todos from the API', async ({ page }) => {
    const cards = page.locator('ul.todo-grid li.todo-card');
    await expect(cards).toHaveCount(baseTodos.length);
    await expect(cards.first().locator('.todo-assignee')).toHaveText(baseTodos[0].assignee);
    await expect(cards.first().locator('.todo-title')).toHaveText(baseTodos[0].title);
  });

  test('allows editing a todo inline', async ({ page }) => {
    const firstCard = page.locator('ul.todo-grid li.todo-card').first();
    await firstCard.locator('button.ghost', { hasText: 'Edit' }).click();

    const titleInput = firstCard.locator('form.edit-form input[name="title"]');
    const assigneeInput = firstCard.locator('form.edit-form input[name="assignee"]');
    await expect(titleInput).toHaveValue(baseTodos[0].title);
    await expect(assigneeInput).toHaveValue(baseTodos[0].assignee);

    const newTitle = 'Spoof NetWatch trace';
    const newAssignee = 'Placide';
    await titleInput.fill(newTitle);
    await assigneeInput.fill(newAssignee);

    const patchRequestPromise = page.waitForRequest(req =>
      req.method() === 'PATCH' && /\/to-do\/\d+$/.test(req.url())
    );

    await firstCard.locator('form.edit-form button[type="submit"]').click();

    const patchRequest = await patchRequestPromise;
    expect(await patchRequest.postDataJSON()).toEqual({
      title: newTitle,
      assignee: newAssignee,
    });

    await expect(firstCard.locator('.todo-title')).toHaveText(newTitle);
    await expect(firstCard.locator('.todo-assignee')).toHaveText(newAssignee);
  });

  test('cancels edits without calling the API', async ({ page }) => {
    let patchTriggered = false;
    page.on('request', req => {
      if (req.method() === 'PATCH' && req.url().includes('/to-do/')) {
        patchTriggered = true;
      }
    });

    const firstCard = page.locator('ul.todo-grid li.todo-card').first();
    await firstCard.locator('button.ghost', { hasText: 'Edit' }).click();

    const titleInput = firstCard.locator('form.edit-form input[name="title"]');
    await titleInput.fill('Ghost change');

    await firstCard.locator('form.edit-form button.ghost', { hasText: 'Cancel' }).click();

    await expect(firstCard.locator('.todo-title')).toHaveText(baseTodos[0].title);
    await expect(firstCard.locator('.todo-assignee')).toHaveText(baseTodos[0].assignee);
    expect(patchTriggered).toBeFalsy();
  });

  test('removes a todo and reflects pending state', async ({ page }) => {
    const cards = page.locator('ul.todo-grid li.todo-card');
    const targetCard = cards.nth(1);
    const removeButton = targetCard.locator('button.danger');

    const deleteRequestPromise = page.waitForRequest(req =>
      req.method() === 'DELETE' && /\/to-do\/\d+$/.test(req.url())
    );

    await removeButton.click();
    await expect(removeButton).toHaveText('Removing...');

    await deleteRequestPromise;
    await expect(cards).toHaveCount(baseTodos.length - 1);
  });
});
