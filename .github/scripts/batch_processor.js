const fs = require('fs');
const path = require('path');
const { GoogleGenerativeAI } = require('@google/generative-ai');

/**
 * CONFIGURACIÓN DE IA - ARQUITECTURA FINOPS
 * Modelo: Gemini 2.0 Flash-Lite (El más económico del catálogo)
 */
const genAI = new GoogleGenerativeAI(process.env.GEMINI_API_KEY);
const queueDir = '.github/ai_queue';

async function main() {
    // 1. Leer la cola de archivos pendientes
    if (!fs.existsSync(queueDir)) {
        console.log("Directorio de cola no encontrado.");
        return;
    }

    const filesInQueue = fs.readdirSync(queueDir).filter(f => f.endsWith('.json') && f.startsWith('req_'));
    
    if (filesInQueue.length === 0) {
        console.log("Cola vacia. Nada que procesar.");
        return;
    }

    let batchContext = "TAREA: DOCUMENTAR EL SIGUIENTE LOTE DE ARCHIVOS.\n\n";
    const pendingTasks = [];

    // 2. Construir el contexto del lote (Batch)
    for (const jsonFile of filesInQueue) {
        const filePath = path.join(queueDir, jsonFile);
        const data = JSON.parse(fs.readFileSync(filePath, 'utf-8'));
        
        if (fs.existsSync(data.file)) {
            const code = fs.readFileSync(data.file, 'utf-8');
            // Delimitadores de entrada claros para evitar confusiones en la IA
            batchContext += `---INPUT_FILE:${data.file}---\n${code}\n---END_INPUT---\n\n`;
            pendingTasks.push(data);
        }
    }

    // 3. Configurar el modelo con tus REGLAS ESTRICTAS
    const model = genAI.getGenerativeModel({ 
        model: "gemini-2.0-flash-lite", 
        systemInstruction: `Eres un Ingeniero de Software Experto y un Analista de Código especializado en documentación técnica en C#/.NET y TypeScript.
        
        ESTÁS PROCESANDO UN LOTE DE ARCHIVOS. Por cada archivo en el input, debes generar una respuesta siguiendo estas reglas:

        REGLA ESTRICTA 1: NO modifiques la lógica, variables, ni estructuras del código.
        REGLA ESTRICTA 2 (CRÍTICA): DEBES devolver el archivo COMPLETO desde la línea 1 hasta la última. ESTÁ ESTRICTAMENTE PROHIBIDO omitir, borrar o recortar directivas 'using', 'namespace', 'import' o encabezados.
        REGLA ESTRICTA 3 (ALCANCE XML): Solo aplica documentación XML (///) a Clases, Interfaces, Propiedades y Métodos. PROHIBIDO documentar variables locales con /// (usa // si es necesario).
        
        ESPECIFICACIONES TÉCNICAS:
        - IDIOMA: Español (ES).
        - FORMATO: Usa <summary>, <param>, <returns>, <exception> y <remarks>.
        - TODOs: Mantén los "TODO:" originales y añade una explicación técnica profesional.
        
        FORMATO DE SALIDA PARA BATCH (OBLIGATORIO):
        Para cada archivo procesado, estructura tu respuesta EXACTAMENTE así:
        FILE_PATH: [ruta/del/archivo]
        FIXED_CONTENT_START
        [Código completo documentado en texto plano]
        FIXED_CONTENT_END

        REGLA DE CIERRE: Devuelve ÚNICAMENTE el contenido bajo este formato, sin markdown (\`\`\`), sin charlas, ni explicaciones adicionales.`,
        generationConfig: {
            temperature: 0, // Cero creatividad para evitar errores sintácticos
            topP: 0.1,
            maxOutputTokens: 65536,
        }
    });

    // 4. Ejecutar la llamada a la IA
    try {
        console.log(`🚀 Procesando lote de ${pendingTasks.length} archivos con Gemini 2.0 Flash-Lite...`);
        const result = await model.generateContent(batchContext);
        const responseText = result.response.text();

        // 5. Guardar respuesta cruda para que el Actor 3 la procese
        fs.writeFileSync(path.join(queueDir, 'raw_response.txt'), responseText);
        // GUARDAR MANIFIESTO PARA LIMPIEZA QUIRÚRGICA
        const processedFilesList = filesInQueue.join(',');
        fs.writeFileSync(path.join(queueDir, 'processed_list.csv'), processedFilesList);
        console.log("✅ Lote procesado exitosamente.");

    } catch (error) {
        console.error("❌ Error crítico en la comunicación con la IA:", error);
        process.exit(1);
    }
}

main();