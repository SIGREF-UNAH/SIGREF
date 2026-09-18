import { existsSync, readdirSync, rmSync } from 'node:fs'
import { join, resolve } from 'node:path'

const apiDir = resolve('src/api')
const preservedEntries = new Set(['mutator'])

if (!existsSync(apiDir)) {
  console.log('src/api does not exist, nothing to clean.')
  process.exit(0)
}

for (const entry of readdirSync(apiDir)) {
  if (preservedEntries.has(entry)) {
    continue
  }

  const target = join(apiDir, entry)

  rmSync(target, {
    recursive: true,
    force: true,
  })

  console.log(`Removed: ${target}`)
}

console.log('API cleanup complete. Preserved: src/api/mutator')