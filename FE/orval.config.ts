import 'dotenv/config'
import { defineConfig } from 'orval'

const apiUrl = process.env.ORVAL_API_URL

if (!apiUrl) {
  throw new Error('ORVAL_API_URL is required')
}

export default defineConfig({
  api: {
    input: {
      target: apiUrl,
    },

    output: {
      mode: 'tags-split',

      target: './src/api/generated/endpoints',

      schemas: {
        path: './src/api/generated/schemas',
        type: 'typescript',
        splitByTags: true,

        routes: {
          default: 'models',
          enum: 'types',
        },
      },

      client: 'react-query',
      httpClient: 'axios',

      formatter: 'prettier',
      clean: true,
      indexFiles: true,
      tagsSplitDeduplication: true,

      override: {
        mutator: {
          path: './src/api/mutator/customInstance.ts',
          name: 'customInstance',
        },

        query: {
          shouldSplitQueryKey: true,
        },
      },
    },
  },
})