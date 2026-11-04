import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { Root } from './Root';

describe('Root', () => {
  it('shows an error that names the variable, and not the app, when the API address is missing', () => {
    render(<Root env={{}} />);

    expect(screen.getByRole('alert')).toHaveTextContent('VITE_API_BASE_URL');
    expect(screen.queryByRole('heading', { name: 'Bank Test' })).not.toBeInTheDocument();
  });

  it('starts the app when the API address is valid', () => {
    render(<Root env={{ VITE_API_BASE_URL: 'http://localhost:5082' }} />);

    expect(screen.getByRole('heading', { name: 'Bank Test' })).toBeInTheDocument();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });
});
