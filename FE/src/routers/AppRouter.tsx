import { Navigate, Route, Routes } from "react-router";
import LocationsRouter from "../features/locations/routers/LocationsRouter";
import { Layout } from "../shared/components";
import { HealthcaresRouter } from "../features/healthcares/routers";
import { HomePage } from "../shared/pages";
import { IncomesRouter } from "../features/incomes/routers";

 export const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
      <Route element={<Layout />}>

        {/* Rutas de Ubicaciones */}
        <Route path="/locations/*" element={<LocationsRouter />} />

        {/* Rutas de Servicios Médicos */}
        <Route path="/healthcares/*" element={<HealthcaresRouter />} />

        {/* Rutas de Ingresos */}
        <Route path="/incomes/*" element={<IncomesRouter />} />

      </Route>
    </Routes>
  );
};