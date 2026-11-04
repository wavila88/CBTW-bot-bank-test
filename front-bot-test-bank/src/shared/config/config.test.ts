import { describe, expect, it } from 'vitest';
import { ConfigError, readConfig } from './config';

describe('readConfig', () => {
  it.each([[undefined], [''], ['   ']])(
    'fails naming VITE_API_BASE_URL when it is missing (%j)',
    (value) => {
      expect(() => readConfig({ VITE_API_BASE_URL: value })).toThrow(ConfigError);
      expect(() => readConfig({ VITE_API_BASE_URL: value })).toThrow(/VITE_API_BASE_URL/);
    },
  );

  it('fails naming VITE_API_BASE_URL when the value is not a URL', () => {
    expect(() => readConfig({ VITE_API_BASE_URL: 'not a url' })).toThrow(/VITE_API_BASE_URL/);
  });

  it('rejects an address that is not http or https', () => {
    expect(() => readConfig({ VITE_API_BASE_URL: 'ftp://files.example.com' })).toThrow(/http/);
  });

  it.each([
    ['http://localhost:5082', 'http://localhost:5082'],
    ['  http://localhost:5082/  ', 'http://localhost:5082'],
    ['https://api.banktest.com/v1/', 'https://api.banktest.com/v1'],
  ])('accepts %j and drops the trailing slash', (value, expected) => {
    expect(readConfig({ VITE_API_BASE_URL: value })).toEqual({ apiBaseUrl: expected });
  });
});
