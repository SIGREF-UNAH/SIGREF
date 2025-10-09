import { Navigate, Route, Routes } from "react-router";
import ListPatients from "../pages/list/ListPatient";
import CreatePatients from "../pages/create/CreatePatients";
import GetByPatientsForm from "../components/ui/GetByFormPatients";
import EditPatients from "../pages/edit/EditPatient";

const PatientsRouter = () => {
  return (
    <Routes>
     <Route path="*" element={<Navigate to="list" replace />} />

      {/* Ruta principal para crear pacientes */}
      <Route path="create" element={<CreatePatients />} />
      {/* Ruta principal para listar pacientes */}
      <Route path="list" element={<ListPatients />} />
      {/* Ruta principal para obtener por id pacientes */}
      <Route path="getby/:id" element={<GetByPatientsForm />} />
      {/* Ruta principal para editar pacientes */}
      <Route path="edit/:id" element={<EditPatients />} />
   
    </Routes>
  );
};

export default PatientsRouter;