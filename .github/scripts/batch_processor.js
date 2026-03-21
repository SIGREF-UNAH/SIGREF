const fs = require('fs');
const path = require('path');
const { GoogleGenerativeAI } = require('@google/generative-ai');

const genAI = new GoogleGenerativeAI(process.env.GEMINI_API_KEY);
const queueFile = '.github/ai_queue/docs_queue.json';
// GUARDAR FUERA DEL REPOSITORIO PARA EVITAR BASURA
const rawResponsePath = '/tmp/raw_response.txt'; 

async function main() {
    if (!fs.existsSync(queueFile)) {
        console.log("Cola no encontrada. Nada que procesar.");
        return;
    }

    const tasks = JSON.parse(fs.readFileSync(queueFile, 'utf-8'));
    
    if (tasks.length === 0) {
        console.log("Cola vacía. Nada que procesar.");
        return;
    }

    let batchContext = "TAREA: DOCUMENTAR EL SIGUIENTE LOTE DE ARCHIVOS.\n\n";
    const validTasks = [];

    for (const data of tasks) {
        if (fs.existsSync(data.file)) {
            const code = fs.readFileSync(data.file, 'utf-8');
            batchContext += `---INPUT_FILE:${data.file}---\n${code}\n---END_INPUT---\n\n`;
            validTasks.push(data);
        }
    }

    if (validTasks.length === 0) return;

    // 3. Configurar el modelo con tus REGLAS ESTRICTAS
    const model = genAI.getGenerativeModel({ 
        model: "gemini-flash-lite-latest", 
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
        console.log(`🚀 Procesando lote de ${validTasks.length} archivos...`);
        const result = await model.generateContent(batchContext);
        const responseText = result.response.text();

        // Guardar la respuesta cruda en la carpeta temporal (FUERA DEL REPO)
        fs.writeFileSync(rawResponsePath, responseText);
        console.log("✅ Lote procesado. Respuesta guardada en memoria temporal.");

    } catch (error) {
        console.error("❌ Error en la comunicación con la IA:", error);
        process.exit(1);
    }
}

main();