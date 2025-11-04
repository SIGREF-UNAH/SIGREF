import html2canvas from "html2canvas";
import jsPDF from "jspdf";
import * as XLSX from "xlsx";
import { message } from "antd";

export interface ExportOptions {
  fileName?: string;
  scale?: number;
  backgroundColor?: string;
  quality?: number;
}

export interface ExcelExportOptions {
  fileName?: string;
  sheetName?: string;
  autoWidth?: boolean;
}

export type ExportFormat = "pdf" | "png" | "jpeg" | "excel";

/**
 * Obtiene el elemento del DOM de forma segura
 */
function getElement(element: HTMLElement | string): HTMLElement {
  const target =
    typeof element === "string" ? document.getElementById(element) : element;

  if (!target) {
    throw new Error(`Elemento no encontrado: ${element}`);
  }

  return target;
}

/**
 * Descarga un blob como archivo
 */
function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

/**
 * Exporta un elemento HTML a PDF
 */
export async function exportToPDF(
  element: HTMLElement | string,
  options: ExportOptions = {}
): Promise<void> {
  const {
    fileName = "documento.pdf",
    scale = 2,
    backgroundColor = "#FFFFFF",
  } = options;

  const target = getElement(element);
  const originalDisplay = target.style.display;

  try {
    message.loading({ content: "Generando PDF...", key: "export" });

    // Asegurar visibilidad
    target.style.display = "block";

    // Esperar carga de imágenes
    const images = target.querySelectorAll("img");
    await Promise.all(
      Array.from(images).map((img) => {
        if (img.complete) return Promise.resolve();
        return new Promise((resolve) => {
          img.onload = resolve;
          img.onerror = resolve;
          setTimeout(resolve, 3000);
        });
      })
    );

    // Esperar repaint
    await new Promise((resolve) => setTimeout(resolve, 300));

    // Obtener dimensiones reales del elemento

    const scrollWidth = target.scrollWidth;
    const scrollHeight = target.scrollHeight;

    const canvas = await html2canvas(target, {
      scale,
      useCORS: true,
      allowTaint: true,
      backgroundColor,
      logging: false,
      width: scrollWidth,
      height: scrollHeight,
      windowWidth: scrollWidth,
      windowHeight: scrollHeight,
      x: 0,
      y: 0,
      scrollX: 0,
      scrollY: 0,
      imageTimeout: 15000,
      onclone: (_clonedDoc, clonedElement) => {
        clonedElement.style.display = "block";
        clonedElement.style.position = "relative";
        clonedElement.style.width = `${scrollWidth}px`;
        clonedElement.style.height = "auto";
        clonedElement.style.minHeight = `${scrollHeight}px`;

        const allElements = clonedElement.querySelectorAll("*");

        allElements.forEach((el: any) => {
          let originalElement = null;

          // ✅ Evitar querySelector si NO tiene ID válido
          if (el.id && /^[A-Za-z][\w\-\:\.]*$/.test(el.id)) {
            originalElement = target.querySelector(`#${el.id}`);
          }

          // Fallback: si no existe original, usar el mismo elemento clonado
          const original = originalElement || el;
          const computedStyle = window.getComputedStyle(original);

          // Aplicación de estilos básicos para evitar que html2canvas los pierda
          el.style.color = computedStyle.color;
          el.style.backgroundColor = computedStyle.backgroundColor;
          el.style.fontSize = computedStyle.fontSize;
          el.style.fontWeight = computedStyle.fontWeight;
          el.style.padding = computedStyle.padding;
          el.style.margin = computedStyle.margin;
          el.style.display = computedStyle.display;
          el.style.textAlign = computedStyle.textAlign;
        });
      },
    });

    if (canvas.width === 0 || canvas.height === 0) {
      throw new Error("El elemento no tiene dimensiones válidas");
    }

    const imgData = canvas.toDataURL("image/png", 1.0);

    // Calcular dimensiones óptimas del PDF
    const pdfWidth = canvas.width / scale;
    const pdfHeight = canvas.height / scale;

    const pdf = new jsPDF({
      orientation: pdfWidth > pdfHeight ? "landscape" : "portrait",
      unit: "px",
      format: [pdfWidth, pdfHeight],
      compress: true,
    });

    pdf.addImage(imgData, "PNG", 0, 0, pdfWidth, pdfHeight, undefined, "FAST");
    pdf.save(fileName.endsWith(".pdf") ? fileName : `${fileName}.pdf`);

    message.success({ content: "PDF generado exitosamente", key: "export" });
  } catch (error) {
    message.error({ content: "Error al generar PDF", key: "export" });
    throw error;
  } finally {
    target.style.display = originalDisplay;
  }
}

/**
 * Exporta un elemento HTML a imagen (PNG o JPEG)
 */
export async function exportToImage(
  element: HTMLElement | string,
  format: "png" | "jpeg" = "png",
  options: ExportOptions = {}
): Promise<void> {
  const {
    fileName = `imagen.${format}`,
    scale = 2,
    backgroundColor = format === "jpeg" ? "#FFFFFF" : null,
    quality = 0.95,
  } = options;

  const target = getElement(element);
  const originalDisplay = target.style.display;

  try {
    message.loading({ content: "Generando imagen...", key: "export" });

    target.style.display = "block";

    // Esperar carga de imágenes
    const images = target.querySelectorAll("img");
    await Promise.all(
      Array.from(images).map((img) => {
        if (img.complete) return Promise.resolve();
        return new Promise((resolve) => {
          img.onload = resolve;
          img.onerror = resolve;
          setTimeout(resolve, 3000);
        });
      })
    );

    await new Promise((resolve) => setTimeout(resolve, 200));

    const canvas = await html2canvas(target, {
      scale,
      useCORS: true,
      allowTaint: true,
      backgroundColor,
      logging: false,
      width: target.scrollWidth,
      height: target.scrollHeight,
    });

    canvas.toBlob(
      (blob) => {
        if (!blob) {
          throw new Error("Error al crear la imagen");
        }

        downloadBlob(
          blob,
          fileName.endsWith(`.${format}`) ? fileName : `${fileName}.${format}`
        );
        message.success({
          content: "Imagen generada exitosamente",
          key: "export",
        });
      },
      `image/${format}`,
      quality
    );
  } catch (error) {
    message.error({ content: "Error al generar imagen", key: "export" });
    throw error;
  } finally {
    target.style.display = originalDisplay;
  }
}

/**
 * Exporta datos a Excel desde un arreglo de objetos o tabla HTML
 */
export function exportToExcel(
  data: any[] | HTMLElement | string,
  options: ExcelExportOptions = {}
): void {
  const {
    fileName = "datos.xlsx",
    sheetName = "Hoja1",
    autoWidth = true,
  } = options;

  try {
    message.loading({ content: "Generando Excel...", key: "export" });

    let worksheet: XLSX.WorkSheet;

    // Si es un arreglo de datos
    if (Array.isArray(data)) {
      worksheet = XLSX.utils.json_to_sheet(data);
    }
    // Si es un elemento HTML (tabla)
    else {
      const target = getElement(data);
      worksheet = XLSX.utils.table_to_sheet(target);
    }

    // Ajustar ancho de columnas automáticamente
    if (autoWidth) {
      const maxWidth = 50;
      const colWidths: { wch: number }[] = [];

      // Calcular anchos basados en el contenido
      const range = XLSX.utils.decode_range(worksheet["!ref"] || "A1");

      for (let col = range.s.c; col <= range.e.c; col++) {
        let maxLen = 10;

        for (let row = range.s.r; row <= range.e.r; row++) {
          const cellAddress = XLSX.utils.encode_cell({ r: row, c: col });
          const cell = worksheet[cellAddress];

          if (cell && cell.v) {
            const len = String(cell.v).length;
            if (len > maxLen) maxLen = len;
          }
        }

        colWidths.push({ wch: Math.min(maxLen + 2, maxWidth) });
      }

      worksheet["!cols"] = colWidths;
    }

    // Crear libro de trabajo y agregar hoja
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, sheetName);

    // Generar y descargar archivo
    XLSX.writeFile(
      workbook,
      fileName.endsWith(".xlsx") ? fileName : `${fileName}.xlsx`
    );

    message.success({ content: "Excel generado exitosamente", key: "export" });
  } catch (error) {
    message.error({ content: "Error al generar Excel", key: "export" });
    throw error;
  }
}


/**
 * Función universal para exportar en cualquier formato
 */
export async function exportData(
  element: HTMLElement | string | any[],
  format: ExportFormat,
  options: ExportOptions & ExcelExportOptions = {}
): Promise<void> {
  switch (format) {
    case "pdf":
      await exportToPDF(element as HTMLElement | string, options);
      break;

    case "png":
      await exportToImage(element as HTMLElement | string, "png", options);
      break;

    case "jpeg":
      await exportToImage(element as HTMLElement | string, "jpeg", options);
      break;

    case "excel":
      exportToExcel(element, options);
      break;

    default:
      throw new Error(`Formato no soportado: ${format}`);
  }
}


/**
 * Hook personalizado para facilitar exportaciones en componentes
 */
export function useExport() {
  const handleExport = async (
    element: HTMLElement | string | any[],
    format: ExportFormat,
    options?: ExportOptions & ExcelExportOptions
  ) => {
    try {
      await exportData(element, format, options);
    } catch (error) {
      message.error("Error al exportar archivo");
    }
  };

  return { exportData: handleExport };
}


/**
 * Convierte una tabla Ant Design a datos para Excel
 */
export function antTableToExcelData(columns: any[], dataSource: any[]): any[] {
  return dataSource.map((record) => {
    const row: any = {};
    columns.forEach((col) => {
      if (col.dataIndex) {
        const key = Array.isArray(col.dataIndex)
          ? col.dataIndex.join(".")
          : col.dataIndex;
        row[col.title || key] = record[col.dataIndex];
      }
    });
    return row;
  });
}

/**
 * Exporta múltiples hojas a un solo archivo Excel
 */
export function exportMultipleSheetsToExcel(
  sheets: Array<{ name: string; data: any[] }>,
  fileName: string = "datos.xlsx"
): void {
  try {
    message.loading({ content: "Generando Excel...", key: "export" });

    const workbook = XLSX.utils.book_new();

    sheets.forEach(({ name, data }) => {
      const worksheet = XLSX.utils.json_to_sheet(data);
      XLSX.utils.book_append_sheet(workbook, worksheet, name);
    });

    XLSX.writeFile(
      workbook,
      fileName.endsWith(".xlsx") ? fileName : `${fileName}.xlsx`
    );

    message.success({ content: "Excel generado exitosamente", key: "export" });
  } catch (error) {
    message.error({ content: "Error al generar Excel", key: "export" });
    throw error;
  }
}
