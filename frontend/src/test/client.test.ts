import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { ApiError, tasksApi } from '../api/client';

const mockFetch = vi.fn();

beforeEach(() => {
  vi.stubGlobal('fetch', mockFetch);
  // Silence localStorage warnings in jsdom
  vi.spyOn(Storage.prototype, 'getItem').mockReturnValue(null);
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe('tasksApi', () => {
  it('throws ApiError with the server message on a non-ok response', async () => {
    mockFetch.mockResolvedValue({
      ok: false,
      status: 401,
      json: async () => ({ error: 'Unauthorized' }),
    });

    await expect(tasksApi.getAll()).rejects.toMatchObject({
      name: 'ApiError',
      status: 401,
      message: 'Unauthorized',
    });
  });

  it('throws ApiError with a fallback message when the body has no error field', async () => {
    mockFetch.mockResolvedValue({
      ok: false,
      status: 500,
      json: async () => ({}),
    });

    const err = await tasksApi.getAll().catch(e => e);
    expect(err).toBeInstanceOf(ApiError);
    expect(err.message).toBe('An unexpected error occurred.');
  });

  it('returns parsed data on a successful response', async () => {
    const task = { id: '1', title: 'Test', status: 'Pending' };
    mockFetch.mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => [task],
    });

    const result = await tasksApi.getAll();
    expect(result).toEqual([task]);
  });
});
