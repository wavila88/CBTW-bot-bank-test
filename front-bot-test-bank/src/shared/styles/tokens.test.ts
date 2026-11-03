import { describe, expect, it } from 'vitest';
import tokensCss from './tokens.css?raw';

// Reads the `--name: value;` pairs of the stylesheet, so the test checks the file that ships.
function readTokens(css: string): Record<string, string> {
  return Object.fromEntries(
    [...css.matchAll(/--([\w-]+):\s*([^;]+);/g)].map(([, name, value]) => [name, value.trim()]),
  );
}

// The values come from the Figma frame 2:679 (the file defines no variables, so they were read from the frame).
describe('design tokens', () => {
  const tokens = readTokens(tokensCss);

  it('has the colors of the design', () => {
    expect(tokens).toMatchObject({
      'color-primary': '#155bc1',
      'color-primary-tint': '#edf4fe',
      'color-navy': '#102e52',
      'color-navy-active': '#23486f',
      'color-background': '#f5f7fb',
      'color-surface': '#ffffff',
      'color-border': '#dfe6ef',
      'color-border-input': '#8795a7',
      'color-text': '#172d47',
      'color-text-muted': '#5c6c80',
      'color-text-on-navy-muted': '#c5d3e4',
    });
  });

  it('uses Inter with the sizes and weights of the design', () => {
    expect(tokens['font-family']).toContain('Inter');
    expect(tokens).toMatchObject({
      'font-size-10': '10px',
      'font-size-11': '11px',
      'font-size-12': '12px',
      'font-size-13': '13px',
      'font-size-14': '14px',
      'font-size-17': '17px',
      'font-size-22': '22px',
      'font-weight-regular': '400',
      'font-weight-medium': '500',
      'font-weight-semibold': '600',
      'font-weight-bold': '700',
    });
  });

  it('has the radii and the layout of the design', () => {
    expect(tokens).toMatchObject({
      'radius-sm': '6px',
      'radius-md': '10px',
      'radius-lg': '12px',
      'radius-tail': '2px',
      'radius-full': '100px',
      'layout-header-height': '80px',
      'layout-sidebar-width': '232px',
      'layout-help-width': '320px',
    });
  });
});
