import { Navigate, Route, Routes } from "react-router";
import { Layout } from "../shared/components";
import { HealthcaresRouter } from "../features/healthcares/routers";
import { DocumentationPage, HomePage, SupportPage } from "../shared/pages";
import { IncomesRouter } from "../features/incomes/routers";
import { PractitionersRouter } from "../features/practitioners/routers";
import { OrganizationsRouter } from "../features/organizations/routers";
import { ReportsRouter } from "../features/reports/routers";
import { EventsRouter } from "../features/events/routers";
import { PatientsRouter } from "../features/patients/routers";
import { LocationsRouter } from "../features/locations/routers";

 export const AppRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="/" replace />} />
      <Route element={<Layout />}>
        {/* Página de Inicio */}
        <Route path="/" element={<HomePage />} />
        
        {/* Página de Soporte (Miembros del Equipo) */}
        <Route path="/support" element={<SupportPage />} />
        
        {/* Página de Documentación */}
        <Route path="/documentation" element={<DocumentationPage />} />
    
        {/* Rutas de Ubicaciones */}
        <Route path="/locations/*" element={<LocationsRouter />} />

        {/* Rutas de Servicios Médicos */}
        <Route path="/healthcares/*" element={<HealthcaresRouter />} />
        {/* Rutas de Ingresos/Fondos */}
        <Route path="/incomes/*" element={<IncomesRouter />} />

        {/* Rutas de Empleados */}
        <Route path="/practitioners/*" element={<PractitionersRouter />} />

        {/* Rutas de Pacientes */}
        <Route path="/patients/*" element={<PatientsRouter />} />

        {/* Rutas de Organizaciones */}
        <Route path="/organizations/*" element={<OrganizationsRouter />} />

        {/* Rutas de Reportes */}
        <Route path="/reports/*" element={<ReportsRouter />} />

        {/* Rutas de Eventos/Logs */}
        <Route path="/events/*" element={<EventsRouter />} />

      </Route>
    </Routes>
  );
};