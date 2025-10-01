import { Navigate, Route, Routes } from "react-router";
import CreateLocationPage from "../pages/create/CreateLocationPage";
import LocationListPage from "../pages/list/LocationListPage";
import LocationDetailsPage from "../pages/list/LocationDetailsPage";

const LocationsRouter = () => {
  return (
    <Routes>
     <Route path="*" element={<Navigate to="list" replace />} />

      {/* Ruta principal para listar ubicaciones */}
      <Route path="list" element={<LocationListPage />} />
      
      {/* Ruta para crear una ubicación */}
      <Route path="create" element={<CreateLocationPage />} />  

      {/* Ruta para ver detalles de una ubicación */}
      <Route path="/details/:id" element={<LocationDetailsPage />} />    
    </Routes>
  );
};

export default LocationsRouter;