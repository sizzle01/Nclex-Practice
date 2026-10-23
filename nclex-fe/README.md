# nclex-fe

Next.js frontend for the NCLEX Practice platform. See the repository root README for how to run
everything locally, and `docs/interfaces.md` section 5 for routes and components.

| Script | What it does |
|---|---|
| `npm run dev` | Dev server on http://localhost:3000 (proxies `/api`, `/healthz`, `/readyz` to `API_URL`, default http://localhost:5080) |
| `npm run lint` | ESLint |
| `npm run typecheck` | Generates Next.js route types, then runs `tsc --noEmit` |
| `npm test` | Vitest + React Testing Library, single run (`npm run test:watch` to watch) |
| `npm run build` | Production build |
| `npm run gen:api` | Regenerates `src/lib/api/schema.d.ts` from the running API's OpenAPI document |
