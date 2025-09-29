import { Route, Routes } from "react-router";
import LocationsRouter from "../features/locations/routers/LocationsRouter";
import { Home } from "../features/auth/pages";

export const AppRouter = () => {
  return (
    <Routes>
      {/* Rutas de locations */}
      <Route path="/locations/*" element={<LocationsRouter />} />

      <Route path="*" element={<Home />} />
    </Routes>
  );
};  
