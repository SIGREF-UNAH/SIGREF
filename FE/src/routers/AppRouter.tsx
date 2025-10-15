import { Navigate, Route, Routes } from "react-router";
import LocationsRouter from "../features/locations/routers/LocationsRouter";
import { Layout } from "../shared/components";
import { HealthcaresRouter } from "../features/healthcares/routers";
import { Home } from "../shared/pages";
import { EmployeesRouter } from "../features/employees/routers";

 export const AppRouter = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="*" element={<Navigate to="/" replace />} />
      <Route element={<Layout />}>

        {/* Rutas de locations */}
        <Route path="/locations/*" element={<LocationsRouter />} />

        {/* Rutas de Servicios Médicos */}
        <Route path="/healthcares/*" element={<HealthcaresRouter />} />

        {/* Rutas de Empleados */}
        <Route path="/employees/*" element={<EmployeesRouter/>}/>

      </Route>
    </Routes>
  );
};