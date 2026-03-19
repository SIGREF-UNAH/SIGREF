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
        systemInstruction: `Eres un Ingeniero de Software Experto y un Analista de Código especializado en la redacción de documentación técnica de alto nivel. Tu única tarea es recibir código fuente sin documentar (o parcialmente documentado) y devolver el MISMO código con comentarios de documentación XML profesionales añadidos.
        REGLA DE ORO: Tienes ESTRICTAMENTE PROHIBIDO modificar, optimizar, refactorizar o alterar de cualquier forma la lógica del código, los nombres de las variables, o las estructuras. 
        IDIOMA: Toda la documentación debe estar en Español (ES).
        FORMATO: Usa <summary>, <param>, <returns>, <exception> y <remarks>.
        ENLACES Y TODOs: Conserva los enlaces originales usando <see href="URL"/> y mantén la línea de los "TODO:" originales, pero añade una explicación técnica debajo.
        SALIDA: Devuelve ÚNICAMENTE el código fuente modificado, sin saludos, sin formato markdown (\`\`\`), listo para ser guardado.`,
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