import { Route, Routes } from "react-router";
import CreateLocationPage from "../pages/create/CreateLocationPage";

const LocationsRouter = () => {
  return (
    <Routes>
      {/* Ruta para crear una ubicación */}
      <Route path="create" element={<CreateLocationPage />} />
    </Routes>
  );
};

export default LocationsRouter;
