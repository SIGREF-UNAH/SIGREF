import { Navigate, Route, Routes } from "react-router";
import LocationsRouter from "../features/locations/routers/LocationsRouter";
import { Layout } from "../shared/components";
import PatientsRouter from "../features/patients/routers/PatientsRouter";
import { HealthcaresRouter } from "../features/healthcares/routers";
import { Home } from "../shared/pages";

 export const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="*" element={<Navigate to="/" replace />} />
      <Route element={<Layout />}>

        {/* Rutas de locations */}
        <Route path="/locations/*" element={<LocationsRouter />} />

        {/* Rutas de pacientes */}
        <Route path="/patients/*" element={<PatientsRouter />} />
        <Route path="*" element={<Home />} />

        {/* Rutas de Servicios Médicos */}
        <Route path="/healthcares/*" element={<HealthcaresRouter />} />

      </Route>
    </Routes>
  );
};