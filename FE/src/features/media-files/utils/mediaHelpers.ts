/**
 * Construye la URL completa para un archivo de media
 * @param relativePath - Ruta relativa del archivo (puede empezar con / o no)
 * @returns URL completa del archivo
 */
export const getMediaUrl = (relativePath?: string | null): string => {
  if (!relativePath) return '';
  
  // Obtener la base URL del API desde tu configuración
  const API_BASE_URL = import.meta.env.VITE_API_URL || window.location.origin;
  
  // Remover el trailing slash si existe
  const baseUrl = API_BASE_URL.endsWith('/') 
    ? API_BASE_URL.slice(0, -1) 
    : API_BASE_URL;
  
  // Si la ruta ya incluye el dominio, devolverla tal cual
  if (relativePath.startsWith('http://') || relativePath.startsWith('https://')) {
    return relativePath;
  }
  
  // Si la ruta empieza con /files/ es una URL del endpoint
  if (relativePath.startsWith('/files/')) {
    return `${baseUrl}${relativePath}`;
  }
  
  // Si la ruta empieza con /media/ es una ruta relativa
  if (relativePath.startsWith('/media/')) {
    return `${baseUrl}${relativePath}`;
  }
  
  // Por defecto, concatenar con la base URL
  return `${baseUrl}${relativePath.startsWith('/') ? '' : '/'}${relativePath}`;
};

/**
 * Formatea el tamaño de archivo en bytes a formato legible
 * @param bytes - Tamaño en bytes
 * @returns String formateado (ej: "1.2 MB")
 */
export const formatFileSize = (bytes?: number): string => {
  if (!bytes) return '0 B';
  
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
};