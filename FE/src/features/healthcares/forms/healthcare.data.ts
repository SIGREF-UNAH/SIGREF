import * as Yup from "yup";

export const healthcareInitValues = {
  name: "",
  abbreviation: "",
  cost: 0,
  comment: "",
  providedBy: {
    reference: "",
    display: ""
  },
  location: [
    {
      reference: "",
      display: ""
    }
  ],
  active: true,
};

export const healthcareValidationSchema = Yup.object().shape({
  name: Yup.string()
    .required("El nombre es requerido")
    .min(3, "El nombre debe tener al menos 3 caracteres")
    .max(100, "El nombre no puede exceder 100 caracteres"),
  abbreviation: Yup.string()
    .required("La abreviatura es requerida")
    .min(2, "La abreviatura debe tener al menos 2 caracteres")
    .max(20, "La abreviatura no puede exceder 20 caracteres"),
  cost: Yup.number()
    .required("El costo es requerido")
    .min(0, "El costo debe ser mayor o igual a 0")
    .typeError("Debe ingresar un número válido"),
  comment: Yup.string()
    .max(255, "La descripción no puede exceder 255 caracteres"),
  providedBy: Yup.object().notRequired(),
  location: Yup.array().notRequired(),
  active: Yup.boolean(),
});
