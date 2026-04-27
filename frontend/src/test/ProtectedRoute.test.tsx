import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, it, expect, vi } from 'vitest';
import { ProtectedRoute } from '../auth/ProtectedRoute';

vi.mock('../auth/AuthContext', () => ({
  useAuth: vi.fn(),
}));

import { useAuth } from '../auth/AuthContext';

const mockUseAuth = useAuth as ReturnType<typeof vi.fn>;

function setup(authState: { user: object | null; isLoading: boolean }) {
  mockUseAuth.mockReturnValue(authState);

  render(
    <MemoryRouter initialEntries={['/protected']}>
      <Routes>
        <Route element={<ProtectedRoute />}>
          <Route path="/protected" element={<div>Protected content</div>} />
        </Route>
        <Route path="/login" element={<div>Login page</div>} />
      </Routes>
    </MemoryRouter>
  );
}

describe('ProtectedRoute', () => {
  it('shows protected content when user is authenticated', () => {
    setup({ user: { userId: '1', name: 'Alice', email: 'a@b.com', token: 'tok' }, isLoading: false });
    expect(screen.getByText('Protected content')).toBeInTheDocument();
  });

  it('redirects to /login when user is not authenticated', () => {
    setup({ user: null, isLoading: false });
    expect(screen.getByText('Login page')).toBeInTheDocument();
  });

  it('shows loading spinner while auth is being resolved', () => {
    setup({ user: null, isLoading: true });
    expect(screen.getByText('Loading…')).toBeInTheDocument();
  });
});
