import { Navigate, Route, Routes } from "react-router";
import { Layout, ProtectedRoute } from "../shared/components";
import { HealthcaresRouter } from "../features/healthcares/routers";
import { DocumentationPage, HomePage, SupportPage } from "../shared/pages";
import { IncomesRouter } from "../features/incomes/routers";
import { PractitionersRouter } from "../features/practitioners/routers";
import { OrganizationsRouter } from "../features/organizations/routers";
import { ReportsRouter } from "../features/reports/routers";
import { EventsRouter } from "../features/events/routers";
import { PatientsRouter } from "../features/patients/routers";
import { LocationsRouter } from "../features/locations/routers";
import { ServiceGroupsRouter } from "../features/service-groups/routers";
import { ShiftsRouter } from "../features/shifts/routers/ShiftsRouter";
import { HospitalRouter } from "../features/hospital/routers";
import { UsersRouter } from "../features/users/routers";
import { CashierSessionsRouter } from "../features/cashier-sessions/routers";
import { SeriesRouter } from "../features/series/router";

 export const AppRouter = () => {
  return (
    <Routes>
      {/* Redireccionar al inicio si la ruta no existe */}
      <Route path="*" element={<Navigate to="/" replace />} />

      {/* Layout principal */}
      <Route element={<Layout />}>

        {/* Página de Inicio */}
        <Route path="/" element={<HomePage />} />
        
        {/* Fondos */}
        <Route path="/incomes/*" element={<IncomesRouter />} />
        
        {/* Sesiones de caja */}
        <Route path="/cashier/*" element={<CashierSessionsRouter />} />

        {/* Ubicaciones / Areas */}
        <Route path="/locations/*" element={<LocationsRouter />} />

        {/* Servicios Médicos */}
        <Route path="/healthcares/*" element={<HealthcaresRouter />} />

        {/* Paquetes de Servicios Médicos */}
        <Route path="/service-groups/*" element={<ServiceGroupsRouter />} />

        {/* Pacientes */}
        <Route path="/patients/*" element={<PatientsRouter />} />

        {/* Empleados */}
        <Route path="/practitioners/*" element={<PractitionersRouter />} />

        {/* Turnos */}
        <Route path="/shifts/*" element={<ShiftsRouter />} />

        {/* Organizaciones */}
        <Route path="/organizations/*" element={<OrganizationsRouter />} />

        {/* Reportes */}
        <Route path="/reports/*" element={<ReportsRouter />} />

        {/* Logs / Eventos */}
        <Route path="/events/*" element={<EventsRouter />} />

        {/* Información del Hospital */}
        <Route path="/hospital/*" element={<HospitalRouter />} />

        {/* Usuarios */}
        <Route path="/users/*" element={<UsersRouter />} />

        {/* Página de Documentación */}
        <Route path="/documentation" element={<DocumentationPage />} />

        <Route path="/series/*" element={<SeriesRouter />} />

        {/* Página de Soporte */}
        <Route path="/support" 
          element={
            <ProtectedRoute action="read" subject="support">
              <SupportPage />
            </ProtectedRoute>
          } 
        />
      </Route>
    </Routes>
  );
};
