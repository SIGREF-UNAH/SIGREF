## 🔗 Navegación  

⬅️ [Volver Atrás](./index.md)  
🏠 [Volver al Inicio](/documentation/index.md)  

# Exportación de Archivos con `useExport / exportData`

Hook y utilidades para exportar datos de React a PDF, imágenes (PNG/JPEG) o Excel. Permite exportar desde tablas HTML, elementos del DOM o arreglos de objetos, con soporte para nombres amigables en columnas.
---

## API: `useExport()`

Hook que facilita la exportación de datos y elementos en cualquier formato soportado.
Retorna:

```ts
{
  exportData: (
    element: HTMLElement | string | any[],
    format: "pdf" | "png" | "jpeg" | "excel",
    options?: ExportOptions & ExcelExportOptions
  ) => Promise<void>
}
```
## Ejemplo Simple
```ts
import { useExport } from "../utils/export";

function ExportPage() {
  const { exportData } = useExport();

  const data = [
    { id: 1, nombre: "Juan", pais: "Honduras" },
    { id: 2, nombre: "Ana", pais: "México" },
  ];

  return (
    <div>
      <button onClick={() => exportData(data, "excel", { fileName: "usuarios.xlsx" })}>
        Exportar Excel
      </button>
    </div>
  );
}
```
## Función exportData(element, format, options)

Función universal para exportar cualquier dato o elemento del DOM.

Parámetros
| Parámetro | Tipo                                     | Requerido | Descripción                                                               |
| --------- | ---------------------------------------- | --------- | ------------------------------------------------------------------------- |
| element   | `HTMLElement` | `string` | `any[]`       | ✅ Sí      | Elemento del DOM, ID del elemento o arreglo de objetos a exportar         |
| format    | `"pdf"` | `"png"` | `"jpeg"` | `"excel"` | ✅ Sí      | Formato de exportación                                                    |
| options   | `ExportOptions & ExcelExportOptions`     | ❌ No      | Configuración adicional (nombre de archivo, escala, color de fondo, etc.) |



## Exportación a PDF
exportToPDF(element, options)

Genera un PDF desde un elemento HTML visible o invisible.

| Opción          | Tipo   | Por Defecto       | Descripción                                 |
| --------------- | ------ | ----------------- | ------------------------------------------- |
| fileName        | string | `"documento.pdf"` | Nombre del archivo generado                 |
| scale           | number | `2`               | Escala de renderizado para mejor resolución |
| backgroundColor | string | `#FFFFFF`         | Color de fondo del PDF                      |

```ts
const { exportData } = useExport();

exportData("miTabla", "pdf", {
  fileName: "reporte.pdf",
  scale: 3,
  backgroundColor: "#fff",
});
```
Notas:
Asegúrate de que el elemento sea visible o se clonará correctamente para el PDF.
Se espera a que las imágenes carguen antes de renderizar.
Permite orientación portrait o landscape automáticamente según dimensiones.

## Exportación a Imagen (PNG/JPEG)
exportToImage(element, format, options)

Genera una imagen a partir de un elemento HTML.
| Opción          | Tipo   | Por Defecto      | Descripción                                  |
| --------------- | ------ | ---------------- | -------------------------------------------- |
| fileName        | string | `"imagen.png"`   | Nombre del archivo generado                  |
| scale           | number | `2`              | Escala de renderizado                        |
| backgroundColor | string | `#FFFFFF` (JPEG) | Color de fondo (solo JPEG por transparencia) |
| quality         | number | `0.95`           | Calidad de la imagen                         |

### Ejemplo PNG
exportData("miSeccion", "png", { fileName: "captura.png", scale: 2 });

### Ejemplo JPEG
exportData("miSeccion", "jpeg", { fileName: "captura.jpeg", scale: 2, quality: 0.9 });

Notas:
Se respetan estilos y colores del DOM.
Se ajusta automáticamente la escala para mejorar resolución.
Ideal para capturas de tablas, gráficas o paneles.

## Exportación a Excel
exportToExcel(data, options)

Genera un archivo Excel desde:
Arreglo de objetos: convierte las propiedades en columnas.
Tabla HTML: convierte los encabezados y celdas automáticamente.
| Opción    | Tipo    | Por Defecto    | Descripción                                     |
| --------- | ------- | -------------- | ----------------------------------------------- |
| fileName  | string  | `"datos.xlsx"` | Nombre del archivo Excel                        |
| sheetName | string  | `"Hoja1"`      | Nombre de la hoja en Excel                      |
| autoWidth | boolean | `true`         | Ajusta automáticamente el ancho de las columnas |

### Ejemplo
```ts
const data = [
  { id: 1, nombre: "Juan", pais: "Honduras" },
  { id: 2, nombre: "Ana", pais: "México" },
];

exportData(data, "excel", { fileName: "usuarios.xlsx", sheetName: "Usuarios" });
```
### Remapear columnas para usuarios
```ts
const columnMap = { id: "ID", nombre: "Nombre completo", pais: "País" };

const friendlyData = data.map(row => {
  const newRow: any = {};
  Object.keys(row).forEach(key => {
    newRow[columnMap[key] || key] = row[key];
  });
  return newRow;
});

exportData(friendlyData, "excel", { fileName: "usuarios_amigable.xlsx" });
```
## Exportación de Múltiples Hojas Excel
exportMultipleSheetsToExcel(sheets, fileName)

Genera un Excel con varias hojas.

| Parámetro | Tipo                              | Requerido      | Descripción                         |
| --------- | --------------------------------- | -------------- | ----------------------------------- |
| sheets    | `{ name: string, data: any[] }[]` | ✅ Sí           | Arreglo de hojas con nombre y datos |
| fileName  | string                            | `"datos.xlsx"` | Nombre del archivo Excel            |

## Ejemplo
```ts
exportMultipleSheetsToExcel([
  { name: "Usuarios", data: usersData },
  { name: "Ventas", data: salesData },
], "informe_completo.xlsx");
```
Notas:
Cada hoja puede tener columnas diferentes.
Permite aplicar remapeo de columnas por hoja antes de exportar.

## Mejores Prácticas

### ✅ DO

Remapear nombres de columnas para Excel antes de exportar.
Mantener elementos visibles o renderizables antes de exportar a PDF/imagen.
Ajustar scale y backgroundColor para mejorar calidad visual.
Usar useExport para un código más limpio y centralizado.
Esperar a que las imágenes carguen antes de exportar PDF/imagen.

### ❌ DON'T

No exportar elementos con display: none directamente.
No usar datos con propiedades complejas sin mapeo para Excel.
No depender de nombres de columna abreviados para el usuario final.