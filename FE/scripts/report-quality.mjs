import { spawnSync } from 'node:child_process'
import { closeSync, mkdtempSync, openSync, readFileSync, rmSync, writeFileSync } from 'node:fs'
import { join, relative, resolve } from 'node:path'
import { tmpdir } from 'node:os'
import { fileURLToPath } from 'node:url'

const severityLabel = {
  1: 'Advertencia',
  2: 'Error',
}

function parseTypeScriptDiagnostics(output) {
  const diagnostics = []

  for (const line of output.split('\n')) {
    const fileDiagnostic = line.match(/^(.+?)\((\d+),(\d+)\):\s+error\s+TS(\d+):\s+(.+)$/)
    const globalDiagnostic = line.match(/^error\s+TS(\d+):\s+(.+)$/)

    if (fileDiagnostic) {
      diagnostics.push({
        file: fileDiagnostic[1],
        line: Number(fileDiagnostic[2]),
        column: Number(fileDiagnostic[3]),
        code: `TS${fileDiagnostic[4]}`,
        message: fileDiagnostic[5],
      })
    } else if (globalDiagnostic) {
      diagnostics.push({
        file: '<configuración>',
        line: null,
        column: null,
        code: `TS${globalDiagnostic[1]}`,
        message: globalDiagnostic[2],
      })
    }
  }

  return diagnostics
}

function relativeFilePath(filePath, rootDir) {
  if (filePath === '<configuración>') return filePath

  const path = relative(rootDir, filePath)
  return path.startsWith('..') ? filePath : path
}

function countBy(items, key) {
  return [...items.reduce((counts, item) => {
    const value = key(item)
    counts.set(value, (counts.get(value) ?? 0) + 1)
    return counts
  }, new Map()).entries()].sort(([left], [right]) => left.localeCompare(right))
}

function markdownTable(headers, rows) {
  const heading = `| ${headers.join(' | ')} |`
  const divider = `| ${headers.map(() => '---').join(' | ')} |`
  const body = rows.map((row) => `| ${row.join(' | ')} |`)

  return [heading, divider, ...body].join('\n')
}

export function createQualityReport({ generatedAt, typecheckOutput, eslintResults, rootDir = process.cwd() }) {
  const typeScriptDiagnostics = parseTypeScriptDiagnostics(typecheckOutput)
    .map((diagnostic) => diagnostic.file === '<configuración>'
      ? diagnostic
      : { ...diagnostic, file: relativeFilePath(resolve(rootDir, diagnostic.file), rootDir) })
  const eslintDiagnostics = eslintResults.flatMap((result) => result.messages.map((message) => ({
    file: relativeFilePath(result.filePath, rootDir),
    line: message.line,
    column: message.column,
    severity: message.severity,
    ruleId: message.ruleId ?? 'eslint/fatal',
    message: message.message,
  })))
  const eslintErrors = eslintDiagnostics.filter((diagnostic) => diagnostic.severity === 2)
  const eslintWarnings = eslintDiagnostics.filter((diagnostic) => diagnostic.severity === 1)
  const allFiles = [
    ...typeScriptDiagnostics.map((diagnostic) => ({ ...diagnostic, severity: 2 })),
    ...eslintDiagnostics,
  ]
  const fileCounts = new Map()

  for (const diagnostic of allFiles) {
    const count = fileCounts.get(diagnostic.file) ?? { errors: 0, warnings: 0 }
    if (diagnostic.severity === 2) count.errors += 1
    if (diagnostic.severity === 1) count.warnings += 1
    fileCounts.set(diagnostic.file, count)
  }

  const errorTypeRows = [
    ...countBy(typeScriptDiagnostics, (diagnostic) => diagnostic.code).map(([code, count]) => [code, 'TypeScript', count]),
    ...countBy(eslintDiagnostics, (diagnostic) => diagnostic.ruleId).map(([rule, count]) => [rule, 'ESLint', count]),
  ]
  const fileRows = [...fileCounts.entries()]
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([file, counts]) => [file, counts.errors, counts.warnings])
  const errorTotal = typeScriptDiagnostics.length + eslintErrors.length
  const warningTotal = eslintWarnings.length
  const formatLocation = (diagnostic) => diagnostic.line === null ? diagnostic.file : `${diagnostic.file}:${diagnostic.line}:${diagnostic.column}`

  return [
    '# SIGREF — Reporte de calidad',
    '',
    `Generado: ${generatedAt.toISOString()}`,
    '',
    '## Resumen',
    '',
    `- Errores totales: ${errorTotal}`,
    `- Advertencias totales: ${warningTotal}`,
    `- Diagnósticos totales: ${errorTotal + warningTotal}`,
    '',
    '## Tipos de errores y advertencias',
    '',
    markdownTable(['Tipo', 'Origen', 'Cantidad'], errorTypeRows),
    '',
    '## Archivos con diagnósticos',
    '',
    markdownTable(['Archivo', 'Errores', 'Advertencias'], fileRows),
    '',
    '## Errores de TypeScript',
    '',
    ...(typeScriptDiagnostics.length === 0 ? ['Sin errores de TypeScript.'] : typeScriptDiagnostics.map((diagnostic) => `- ${formatLocation(diagnostic)} — ${diagnostic.code}: ${diagnostic.message}`)),
    '',
    '## Diagnósticos de ESLint',
    '',
    ...(eslintDiagnostics.length === 0 ? ['Sin diagnósticos de ESLint.'] : eslintDiagnostics.map((diagnostic) => `- ${formatLocation(diagnostic)} — ${severityLabel[diagnostic.severity]} ${diagnostic.ruleId}: ${diagnostic.message}`)),
    '',
  ].join('\n')
}

function run(command, args) {
  const directory = mkdtempSync(join(tmpdir(), 'sigref-quality-'))
  const outputPath = join(directory, 'command-output.txt')
  const outputFile = openSync(outputPath, 'w')

  try {
    const result = spawnSync(command, args, { stdio: ['ignore', outputFile, outputFile] })

    return {
      output: readFileSync(outputPath, 'utf8'),
      exitCode: result.status ?? 1,
    }
  } finally {
    closeSync(outputFile)
    rmSync(directory, { force: true, recursive: true })
  }
}

export function generateQualityReport() {
  const typecheck = run(process.execPath, [resolve('node_modules/typescript/bin/tsc'), '-b', '--pretty', 'false'])
  const eslint = run(process.execPath, [resolve('node_modules/eslint/bin/eslint.js'), '.', '--format', 'json'])
  let eslintResults

  try {
    eslintResults = JSON.parse(eslint.output)
  } catch {
    eslintResults = [{
      filePath: '<configuración>',
      messages: [{ line: null, column: null, severity: 2, ruleId: 'eslint/report-parse', message: eslint.output.trim() || 'ESLint no produjo salida JSON.' }],
    }]
  }

  const report = createQualityReport({
    generatedAt: new Date(),
    typecheckOutput: typecheck.output,
    eslintResults,
  })

  writeFileSync(resolve('lint-output.txt'), report)
  process.stdout.write('Reporte actualizado: lint-output.txt\n')
  return typecheck.exitCode !== 0 || eslint.exitCode !== 0 ? 1 : 0
}

if (fileURLToPath(import.meta.url) === process.argv[1]) {
  process.exitCode = generateQualityReport()
}
