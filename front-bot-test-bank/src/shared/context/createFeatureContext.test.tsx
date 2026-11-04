import { act, renderHook } from '@testing-library/react';
import type { ReactNode } from 'react';
import { describe, expect, it, vi } from 'vitest';
import { createFeatureContext } from './createFeatureContext';

interface CounterState {
  count: number;
}

type CounterAction = { type: 'increment' } | { type: 'add'; amount: number };

function counterReducer(state: CounterState, action: CounterAction): CounterState {
  switch (action.type) {
    case 'increment':
      return { count: state.count + 1 };
    case 'add':
      return { count: state.count + action.amount };
  }
}

const counter = createFeatureContext('Counter', counterReducer, { count: 0 });

function wrapper({ children }: { children: ReactNode }) {
  return <counter.Provider>{children}</counter.Provider>;
}

describe('createFeatureContext', () => {
  it('returns the state and the dispatch inside the provider', () => {
    const { result } = renderHook(() => counter.useFeature(), { wrapper });

    expect(result.current.state).toEqual({ count: 0 });
    expect(typeof result.current.dispatch).toBe('function');
  });

  it('updates the state when an action is dispatched', () => {
    const { result } = renderHook(() => counter.useFeature(), { wrapper });

    act(() => result.current.dispatch({ type: 'increment' }));
    act(() => result.current.dispatch({ type: 'add', amount: 5 }));

    expect(result.current.state).toEqual({ count: 6 });
  });

  it('keeps the state of each provider apart', () => {
    const first = renderHook(() => counter.useFeature(), { wrapper });
    const second = renderHook(() => counter.useFeature(), { wrapper });

    act(() => first.result.current.dispatch({ type: 'increment' }));

    expect(first.result.current.state).toEqual({ count: 1 });
    expect(second.result.current.state).toEqual({ count: 0 });
  });

  it('fails naming the provider when the hook is read outside it', () => {
    // React reports the thrown error on the console as well; it is expected here.
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {});

    expect(() => renderHook(() => counter.useFeature())).toThrow('CounterProvider');

    consoleError.mockRestore();
  });
});
