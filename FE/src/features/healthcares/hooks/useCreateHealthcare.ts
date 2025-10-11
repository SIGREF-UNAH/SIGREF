import { useNavigate } from "react-router";
import { useState } from "react";
import { healthcareInitValues, healthcareValidationSchema } from "../forms";
import { useFormik } from "formik";
import type { HealthcareDto } from "../../../api/models";

export function useCreateHealthcare() {
  const navigate = useNavigate();
  const [isPending, setIsPending] = useState(false);

  // Validación del formulario con Formik
  const formik = useFormik<HealthcareDto>({
    initialValues: healthcareInitValues,
    validationSchema: healthcareValidationSchema,
    onSubmit: async (values) => {
      setIsPending(true);

      try {
        // Llamada a la API
        // const result = await createHealthcare(values);
        const result = { healthcare: values, status: true, message: "Actividad creada correctamente" };
        console.log("Healthcare Service to create:", result.healthcare);

        if (!result.status) {
          console.error("Error al crear el servicio médico: ", result.message);
          return;
        }

        navigate("/healthcares");
        
      } catch (error) {
        console.error(error);
      } finally {
        setIsPending(false);
      } 
    },
  });

  return {
    formik,
    isPending,
  };
}
