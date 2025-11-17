export interface Document {
  id: number;
  title: string;
  description: string;
  type: "pdf" | "word" | "excel" | "text";
  size: string;
  date: string;
  url: string;
}

export const documents: Document[] = [
  {
    id: 1,
    title: "Informe Técnico del Proyecto",
    description: "Documento detallado con todas las especificaciones técnicas y arquitectura del sistema.",
    type: "pdf",
    size: "2.4 MB",
    date: "15/10/2024",
    url: "/docs/informe-tecnico.pdf",
  },
];
