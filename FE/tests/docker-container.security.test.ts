import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'

const root = resolve(import.meta.dirname, '..')
const readProjectFile = (file: string) => readFileSync(resolve(root, file), 'utf8')

describe('production container hardening', () => {
  it('uses a reproducible build and serves only on an unprivileged port', () => {
    const dockerfile = readProjectFile('Dockerfile')

    expect(dockerfile).toMatch(/RUN(?:\s+--mount=[^\n]+)?\s+npm ci/)
    expect(dockerfile).toContain('RUN npm run build')
    expect(dockerfile).not.toContain('npm run build:prod')
    expect(dockerfile).toContain('EXPOSE 8080')
    expect(dockerfile).toContain('USER nginx')
    expect(dockerfile).not.toMatch(/^EXPOSE\s+80\b/m)
  })

  it('does not send secrets or local build artifacts to Docker', () => {
    const dockerignore = readProjectFile('.dockerignore')

    expect(dockerignore).toContain('.env')
    expect(dockerignore).toContain('node_modules')
    expect(dockerignore).toContain('.git')
    expect(dockerignore).toContain('dist')
  })

  it('keeps Nginx on the same unprivileged port without extra listeners', () => {
    const nginx = readProjectFile('nginx.conf')

    expect(nginx).toMatch(/^\s*listen 8080;/m)
    expect(nginx).not.toMatch(/^\s*listen \[::\]:/m)
    expect(nginx).not.toMatch(/^\s*listen 80;/m)
  })
})
