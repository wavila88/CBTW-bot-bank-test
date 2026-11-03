# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> **IMPORTANT:** Before any change, read `.sdd/project.md`. Its rules are mandatory: configuration values come from environment variables (never hardcoded), the feature-based dependency rules apply, and tests follow its conventions. A change that breaks them is not done.

## Overview

The front of the Bank Test support chat: React with TypeScript, feature-based architecture, React Context for state. It talks to the API of `../back-bot-test-bank` (`POST /Chat`). The design is in Figma (see `.sdd/project.md`). The specs are in `.sdd/specs/`: `001-front-architecture` (the foundation) and `002-chat-experience` (the chat).

## Commands

Run them from this folder.

- Install: `npm install`
- Run: `npm run dev` (the address is printed in the terminal)
- Build: `npm run build` (type check and bundle)
- Test: `npm test` (Vitest with React Testing Library; no real calls to the API)

## Structure

- `src/app/`: the entry, the providers and the page that composes the features.
- `src/features/<feature>/`: one folder per feature; other code uses a feature only through its `index.ts`.
- `src/shared/`: code used by more than one feature; it imports nothing from `features` or `app`.
