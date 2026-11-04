import { createContext, useContext, useReducer, type Dispatch, type ReactNode, type Reducer } from 'react';

export interface FeatureContextValue<State, Action> {
  state: State;
  dispatch: Dispatch<Action>;
}

/**
 * Creates the state of one feature: a Context provider that holds it with a reducer, and the hook that reads it.
 * Each feature calls this once in its own `context/` folder and exposes the provider and the hook from its `index.ts`.
 * The provider is composed in `src/app` (AppProviders).
 */
export function createFeatureContext<State, Action>(
  name: string,
  reducer: Reducer<State, Action>,
  initialState: State,
) {
  const Context = createContext<FeatureContextValue<State, Action> | null>(null);
  Context.displayName = name;

  function Provider({ children }: { children: ReactNode }) {
    const [state, dispatch] = useReducer(reducer, initialState);
    return <Context.Provider value={{ state, dispatch }}>{children}</Context.Provider>;
  }
  Provider.displayName = `${name}Provider`;

  function useFeature(): FeatureContextValue<State, Action> {
    const value = useContext(Context);
    if (value === null) {
      throw new Error(`The ${name} state was read outside its provider: wrap the component in <${name}Provider>.`);
    }
    return value;
  }

  return { Provider, useFeature };
}
