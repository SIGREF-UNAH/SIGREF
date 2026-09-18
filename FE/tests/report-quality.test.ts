import { describe, expect, it } from 'vitest'
import { createQualityReport } from '../scripts/report-quality.mjs'

describe('createQualityReport', () => {
  it('summarizes TypeScript and ESLint diagnostics before listing them', () => {
    const report = createQualityReport({
      generatedAt: new Date('2026-09-18T16:00:00Z'),
      rootDir: '/app',
      typecheckOutput: [
        'src/app.ts(10,5): error TS2322: Type \'string\' is not assignable to type \'number\'.',
        'src/app.ts(12,3): error TS2322: Type \'string\' is not assignable to type \'number\'.',
        'src/user.ts(2,1): error TS7006: Parameter \'id\' implicitly has an \'any\' type.',
      ].join('\n'),
      eslintResults: [
        {
          filePath: '/app/src/app.ts',
          errorCount: 1,
          warningCount: 1,
          messages: [
            { line: 20, column: 4, severity: 2, ruleId: '@typescript-eslint/no-unsafe-assignment', message: 'Unsafe assignment.' },
            { line: 21, column: 2, severity: 1, ruleId: '@typescript-eslint/no-explicit-any', message: 'Unexpected any.' },
          ],
        },
      ],
    })

    expect(report).toContain('# SIGREF — Reporte de calidad')
    expect(report).toContain('Errores totales: 4')
    expect(report).toContain('Advertencias totales: 1')
    expect(report).toContain('| TS2322 | TypeScript | 2 |')
    expect(report).toContain('| @typescript-eslint/no-unsafe-assignment | ESLint | 1 |')
    expect(report).toContain('| src/app.ts | 3 | 1 |')
    expect(report).toContain('## Errores de TypeScript')
    expect(report).toContain('TS7006')
    expect(report).toContain('## Diagnósticos de ESLint')
    expect(report).toContain('Unexpected any.')
  })
})
