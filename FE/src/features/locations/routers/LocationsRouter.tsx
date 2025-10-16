import { Navigate, Route, Routes } from "react-router";
import EditLocations from "../pages/EditLocation";
import LocationDetailsPage from "../pages/LocationDetailsPage";
import CreateLocationPage from "../pages/CreateLocationPage";
import LocationListPage from "../pages/LocationListPage";

const LocationsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      {/* Ruta principal para listar ubicaciones */}
      <Route path="list" element={<LocationListPage />} />
      
      {/* Ruta para crear una ubicación */}
      <Route path="create" element={<CreateLocationPage />} />  

      {/* Ruta para editar una ubicación */}
      <Route path="edit/:id" element={<EditLocations />} />

      {/* Ruta para ver detalles de una ubicación */}
      <Route path="/details/:id" element={<LocationDetailsPage />} />    

    </Routes>
  );
};

export default LocationsRouter;