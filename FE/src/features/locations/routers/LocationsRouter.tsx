import { Route, Routes } from "react-router";
import CreateLocationPage from "../pages/create/CreateLocationPage";
import LocationListPage from "../pages/list/LocationListPage";

const LocationsRouter = () => {
  return (
    <Routes>
      {/* Ruta principal para listar ubicaciones */}
      <Route path="list" element={<LocationListPage />} />
      
      {/* Ruta para crear una ubicación */}
      <Route path="create" element={<CreateLocationPage />} />      
    </Routes>
  );
};

export default LocationsRouter;