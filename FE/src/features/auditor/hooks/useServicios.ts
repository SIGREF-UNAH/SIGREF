import { useState, useEffect } from "react";
import { http } from "../../../shared/utils/http";
import { Servicio } from "../components/ui/ServiciosTable";

export const useServicios = () => {
  const [servicios, setServicios] = useState<Servicio[]>([]);
  const [loading, setLoading] = useState(false);

  // Datos de ejemplo - Estos serían reemplazados por la llamada a la API
  const serviciosEjemplo = [
    { id: "1", abreviatura: "RX", nombre: "Rayos X", areaAsistencial: "Consulta Externa", costo: 100.00 },
    { id: "2", abreviatura: "ES", nombre: "Examen de Sangre", areaAsistencial: "Contancias", costo: 100.00 },
    { id: "3", abreviatura: "EP", nombre: "Examen de Prostata", areaAsistencial: "Obstetricia", costo: 200.00 },
    { id: "4", abreviatura: "CM", nombre: "Cita Medica", areaAsistencial: "Laboratorio", costo: 150.00 },
    { id: "5", abreviatura: "RP", nombre: "Radiografía Pulmonar", areaAsistencial: "RX", costo: 250.00 },
    { id: "6", abreviatura: "EC", nombre: "Examen de COVID", areaAsistencial: "Examenes Especiales", costo: 80.00 },
    { id: "7", abreviatura: "EO", nombre: "Examen de Orina", areaAsistencial: "Obstetricia", costo: 50.00 },
    { id: "8", abreviatura: "VR", nombre: "Vacuna contra la rabia", areaAsistencial: "Laboratorio", costo: 100.00 },
    { id: "9", abreviatura: "RPV", nombre: "Radiografía Pelvica", areaAsistencial: "Laboratorio", costo: 400.00 },
  ];

  const fetchServicios = async () => {
    setLoading(true);
    
    try {
      // Aquí iría la llamada al backend cuando esté disponible
      // const response = await http.get('/api/servicios');
      // setServicios(response.data);
      
      // Por ahora usamos datos de ejemplo
      setTimeout(() => {
        setServicios(serviciosEjemplo);
        setLoading(false);
      }, 500);
    } catch (error) {
      console.error('Error al cargar servicios:', error);
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchServicios();
  }, []);

  return {
    servicios,
    loading,
    fetchServicios
  };
};