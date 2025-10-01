require('dotenv').config();

module.exports = {
  api: {
    input: {
      target: process.env.ORVAL_API_URL,
    },
    output: {
      // Generar un archivo por cada controller
      mode: "tags-split",
      // Carpeta donde se generarán los hooks
      target: "src/api",
      // Cliente HTTP: axios 
      client: "react-query",
      // Carpeta para los modelos/types generados
      schemas: "src/api/models",
      prettier: true,
      override: {
        // Usar un mutator
        mutator: {
          path: "./src/api/mutator/customInstance.ts",
          name: "customInstance",
        },
        // Generar queryKeys como arrays (Para invalidar la cache)
        shouldSplitQueryKey: true,
      },
    },
  },
};
