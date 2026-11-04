import '@fontsource-variable/inter';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { Root } from './app/Root';
import './shared/styles/tokens.css';
import './shared/styles/global.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Root env={import.meta.env} />
  </StrictMode>,
);
