import { useNavigate, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { useFormik } from "formik";
import { healthcareInitValues, healthcareValidationSchema } from "../forms";

export function useUpdateHealthcare() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [isPending, setIsPending] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  // Validación del formulario con Formik
  const formik = useFormik({
    initialValues: healthcareInitValues,
    validationSchema: healthcareValidationSchema,
    onSubmit: async (values) => {
      setIsPending(true);

      try {
        // Llamada a la API
        // const result = await updateHealthcare(id, values);
        const result = { 
          healthcare: values, 
          status: true, 
          message: "Servicio actualizado correctamente" 
        };
        console.log("Healthcare Service to update:", result.healthcare);

        if (!result.status) {
          console.error("Error al actualizar el servicio médico: ", result.message);
          return;
        }

        navigate("/healthcares");
        
      } catch (error) {
        console.error(error);
      } finally {
        setIsPending(false);
      } 
    },
    enableReinitialize: true,
  });

  useEffect(() => {
    const fetchHealthcareData = async () => {
      try {
        setIsLoading(true);
        
        // TODO: Reemplazar con llamada real al backend
        // const response = await getHealthcareById(id);
        
        // Simulación de llamada al backend con delay
        await new Promise((resolve) => setTimeout(resolve, 500));
        
        // Mock data - simula la respuesta del backend
        const mockResponse = {
          identifier: null,
          active: true,
          name: "Radiografía Dental",
          abbreviation: "RD",
          comment: "Servicio de radiografía dental completo",
          cost: 300.0,
          specialty: null,
          providedBy: {
            type: null,
            identifier: null,
            reference: "Organization/1702",
            display: "Hospital de Occidente",
          },
          location: [
            {
              type: null,
              identifier: null,
              reference: "Location/1",
              display: "Consultoría Externa",
            },
            {
              type: null,
              identifier: null,
              reference: "Location/2",
              display: "Emergencias",
            },
          ],
        };

        // Establecer los valores en formik
        formik.setValues({
          name: mockResponse.name,
          abbreviation: mockResponse.abbreviation,
          cost: mockResponse.cost,
          comment: mockResponse.comment,
          providedBy: mockResponse.providedBy,
          location: mockResponse.location,
          active: mockResponse.active,
        });
      } catch (error) {
        console.error("Error al cargar el servicio:", error);
        // TODO: Mostrar notificación de error
        // message.error("Error al cargar el servicio");
      } finally {
        setIsLoading(false);
      }
    };

    if (id) {
      fetchHealthcareData();
    }
  }, [id]);

  return {
    formik,
    isPending,
    isLoading,
  };
}