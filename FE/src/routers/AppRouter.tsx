import { Route, Routes } from "react-router";
import LocationsRouter from "../features/locations/routers/LocationsRouter";
import { Home } from "../features/auth/pages";
import { Layout } from "../shared/components";

 export const AppRouter = () => {
  return (
    <Routes>
      <Route element={<Layout />}>
        {/* Rutas de locations */}
        <Route path="/locations/*" element={<LocationsRouter />} />
        
        <Route path="*" element={<Home />} />
      </Route>
    </Routes>
  );
};