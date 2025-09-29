import { Route, Routes } from "react-router";
import AuthRouter from "../features/auth/routers/AuthRouter";
import LocationsRouter from "../features/locations/routers/LocationsRouter";

export const AppRouter = () => {
  return (
    <Routes>
      {/* Rutas de locations */}
      <Route path="/locations/*" element={<LocationsRouter />} />

      {/* Rutas de auth */}
      <Route path="/*" element={<AuthRouter />} />
    </Routes>
  );
};
