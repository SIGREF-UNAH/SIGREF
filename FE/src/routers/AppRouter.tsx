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
import { CashierRouteGuard } from "../features/cashier-sessions/components";
import OpenCashierSessionPage from "../features/cashier-sessions/pages/OpenCashierSessionPage";

 export const AppRouter = () => {
  return (
    <Routes>
      {/* Redirección a "/" si la ruta no existe */}
      <Route path="*" element={<Navigate to="/" replace />} />

      {/* Ruta para abrir sesión de cajero */}
      <Route element={<CashierRouteGuard requiresActiveSession={false} requiresCashierRole={true} />}>
        <Route path="/cashier/open-session" element={<OpenCashierSessionPage />} />
      </Route>

      {/* Rutas que requieren sesión activa de cajero */}
      <Route element={<CashierRouteGuard requiresActiveSession={true} requiresCashierRole={true} />}>
        {/* TODO: AGREGAR RUTA PARA CERRAR SESIÓN DE CAJERO */}
      </Route>

      {/* Layout principal */}
      <Route element={<Layout />}>

        {/* Público */}
        <Route path="/" element={<HomePage />} />
        <Route path="/documentation" element={<DocumentationPage />} />

        {/* Ubicaciones */}
        <Route
          path="/locations/*"
          element={
            <ProtectedRoute action="read" subject="locations">
              <LocationsRouter />
            </ProtectedRoute>
          }
        />

        {/* Servicios Médicos */}
        <Route
          path="/healthcares/*"
          element={
            <ProtectedRoute action="read" subject="healthcares">
              <HealthcaresRouter />
            </ProtectedRoute>
          }
        />

        {/* Grupos de Servicios */}
        <Route
          path="/service-groups/*"
          element={
            <ProtectedRoute action="read" subject="healthcares">
              <ServiceGroupsRouter />
            </ProtectedRoute>
          }
        />

        {/* Fondos / Incomes */}
        <Route
          path="/incomes/*"
          element={
            <ProtectedRoute action="read" subject="incomes">
              <IncomesRouter />
            </ProtectedRoute>
          }
        />

        {/* Empleados */}
        <Route
          path="/practitioners/*"
          element={
            <ProtectedRoute action="read" subject="practitioners">
              <PractitionersRouter />
            </ProtectedRoute>
          }
        />

        {/* Pacientes */}
        <Route
          path="/patients/*"
          element={
            <ProtectedRoute action="read" subject="patients">
              <PatientsRouter />
            </ProtectedRoute>
          }
        />

        {/* Organizaciones */}
        <Route
          path="/organizations/*"
          element={
            <ProtectedRoute action="read" subject="organizations">
              <OrganizationsRouter />
            </ProtectedRoute>
          }
        />

        {/* Reportes */}
        <Route
          path="/reports/*"
          element={
            <ProtectedRoute action="read" subject="reports">
              <ReportsRouter />
            </ProtectedRoute>
          }
        />

        {/* Logs / Eventos */}
        <Route
          path="/events/*"
          element={
            <ProtectedRoute action="read" subject="events">
              <EventsRouter />
            </ProtectedRoute>
          }
        />

        {/* Información del Hospital */}
        <Route
          path="/hospital/*"
          element={
            <ProtectedRoute action="read" subject="hospital">
              <HospitalRouter />
            </ProtectedRoute>
          }
        />

        {/* Usuarios */}
        <Route
          path="/users/*"
          element={
            <ProtectedRoute action="read" subject="users">
              <UsersRouter />
            </ProtectedRoute>
          }
        />

        {/* Turnos */}
        <Route
          path="/shifts/*"
          element={
            <ProtectedRoute action="read" subject="shifts">
              <ShiftsRouter />
            </ProtectedRoute>
          }
        />
        {/* Soporte */}
        <Route 
          path="/support" 
          element={
            <ProtectedRoute action="read" subject="support">
              <SupportPage />
            </ProtectedRoute>
          } />

      </Route>
    </Routes>
  );
};
