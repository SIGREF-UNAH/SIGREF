import { Navigate, Route, Routes } from "react-router";
import { Home } from "../features/auth/pages";
import { ExampleRouter } from "../features/example/routers";
import LocationsRouter from "../features/locations/routers/LocationsRouter";
import { Layout } from "../shared/components";
import PatientsRouter from "../features/patients/routers/PatientsRouter";

 export const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/example/*" element={<ExampleRouter />} />
      <Route path="*" element={<Navigate to="/" replace />} />
      <Route element={<Layout />}>
        {/* Rutas de locations */}
        <Route path="/locations/*" element={<LocationsRouter />} />
        {/* Rutas de pacientes */}
        <Route path="/patients/*" element={<PatientsRouter />} />
        <Route path="*" element={<Home />} />
      </Route>
    </Routes>
  );
};