import { Route, Routes } from "react-router";
import CreateLocationPage from "../pages/create/CreateLocationPage";
import LocationListPage from "../pages/list/LocationListPage";
import EditLocation from "../pages/edit/EditLocation";

const LocationsRouter = () => {
  return (
    <Routes>
      {/* Ruta principal para listar ubicaciones */}
      <Route path="list" element={<LocationListPage />} />
      
      {/* Ruta para crear una ubicación */}
      <Route path="create" element={<CreateLocationPage />} />      

      {/* Ruta para editar una ubicación */}
      <Route path="edit/:id" element={<EditLocation />} />
    </Routes>
  );
};

export default LocationsRouter;