const fs = require('fs');
const { GoogleGenerativeAI } = require('@google/generative-ai');

// 1. Inicializar la API con el secreto de GitHub
const genAI = new GoogleGenerativeAI(process.env.GEMINI_API_KEY);

async function main() {
    const issueBody = process.env.ISSUE_BODY;
    
    // 2. Extraer rutas de archivos del cuerpo del Issue
    // Buscará líneas que digan exactamente: "Archivo: ruta/del/archivo.cs"
    const regex = /Archivo:\s*(.+)/g;
    let match;
    const archivos = [];
    while ((match = regex.exec(issueBody)) !== null) {
        archivos.push(match[1].trim());
    }

    if (archivos.length === 0) {
        console.log("No se encontraron archivos etiquetados con 'Archivo:' en el Issue.");
        return;
    }

    // 3. Configurar el modelo Flash (rápido, barato y perfecto para esto)
    const model = genAI.getGenerativeModel({ 
        model: 'gemini-2.5-flash',
        systemInstruction: `Eres un Ingeniero de Software Experto y un Analista de Código especializado en documentación técnica en C#/.NET y TypeScript.
        REGLA ESTRICTA 1: NO modifiques la lógica, variables, ni estructuras del código.
        REGLA ESTRICTA 2 (CRÍTICA): DEBES devolver el archivo COMPLETO desde la línea 1 hasta la última. ESTÁ ESTRICTAMENTE PROHIBIDO omitir, borrar o recortar las directivas 'using', 'namespace', 'import' o cualquier encabezado del archivo original.
        REGLA ESTRICTA 3 (ALCANCE XML): Solo aplica documentación XML (///) a Clases, Interfaces, Propiedades y Métodos. ESTÁ ESTRICTAMENTE PROHIBIDO documentar variables locales dentro de los métodos con ///. Si necesitas explicar algo dentro de un método, usa comentarios normales (//).
        IDIOMA: Español (ES).
        FORMATO: Usa <summary>, <param>, <returns>, <exception> y <remarks>.
        TODOs: Mantén los "TODO:" originales y añade una explicación técnica.
        SALIDA: Devuelve ÚNICAMENTE el código en texto plano, sin markdown (\`\`\`), listo para guardar.`,
        generationConfig: {
            temperature: 0.0, // Cero creatividad, máxima precisión
            topP: 0.1,
            maxOutputTokens: 65536,
        }
    });

    // 4. Procesar cada archivo encontrado
    for (const ruta of archivos) {
        if (fs.existsSync(ruta)) {
            console.log(`Procesando archivo: ${ruta}`);
            const codigoOriginal = fs.readFileSync(ruta, 'utf-8');
            
            try {
                const result = await model.generateContent(codigoOriginal);
                let codigoDocumentado = result.response.text();
                
                // Limpieza por si el modelo devuelve bloques de markdown accidentalmente
                codigoDocumentado = codigoDocumentado.replace(/^```[a-z]*\n/gm, '').replace(/```$/gm, '');

                // 5. Sobrescribir el archivo original
                fs.writeFileSync(ruta, codigoDocumentado.trim(), 'utf-8');
                console.log(`✅ Éxito: Documentación generada para ${ruta}`);
            } catch (error) {
                console.error(`❌ Error procesando ${ruta}:`, error);
            }
        } else {
            console.log(`⚠️ Advertencia: El archivo ${ruta} no existe en el repositorio.`);
        }
    }
}

main();