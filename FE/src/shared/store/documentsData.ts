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
    title: "Manual de Usuario",
    description: "Guía de uso del sistema para usuarios finales",
    type: "word",
    size: "57 KB",
    date: "03/12/2025",
    url: "https://docs.google.com/document/d/1MLNP7Q6HhwoQJt9bM1_nQIXTZLzZAiur/edit?usp=sharing&ouid=108422404104592509892&rtpof=true&sd=true",
  },
  {
    id: 2,
    title: "Manual Técnico",
    description: "Estructura de datos y especificaciones técnicas",
    type: "word",
    size: "56 KB",
    date: "03/12/2025",
    url: "https://docs.google.com/document/d/1HmxqqgNkJyK8NABjFj4kaTUqsrr60s7N/edit?usp=sharing&ouid=108422404104592509892&rtpof=true&sd=true",
  },
  {
    id: 3,
    title: "Manual de Implementación",
    description: "Guía de implementación del sistema",
    type: "word",
    size: "39 KB",
    date: "03/12/2025",
    url: "https://docs.google.com/document/d/1oHFusHeGGraIrH9zEnl9GpRiunFh2Xis/edit?usp=sharing&ouid=108422404104592509892&rtpof=true&sd=true",
  },
];
