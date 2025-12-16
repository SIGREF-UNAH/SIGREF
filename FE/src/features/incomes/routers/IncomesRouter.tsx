import { Navigate, Route, Routes } from "react-router";
import { CashClosingPage, CreateIncomePage, HistoryClosingPage, ListIncomePage } from "../pages";
import { ProtectedRoute } from "../../../shared/components";
import { CashierRouteGuard } from "../../cashier-sessions/components";

export const IncomesRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      <Route 
        path="/list" 
        element={
          <ProtectedRoute action="read" subject="incomes">
            <ListIncomePage />
          </ProtectedRoute>
        } 
      />

      {/* Ruta que requiere sesión activa de cajero */}
      <Route element={<CashierRouteGuard requiresActiveSession={true} requiresCashierRole={true} />}>
        <Route 
          path="/create" 
          element={
            <ProtectedRoute action="create" subject="incomes">
              <CreateIncomePage />
            </ProtectedRoute>
          } 
        />
      </Route>

      {/* Ruta que requiere sesión activa de cajero */}
      <Route element={<CashierRouteGuard requiresActiveSession={true} requiresCashierRole={true} />}>
        <Route 
          path="/close" 
          element={
            <ProtectedRoute action="update" subject="incomes">
              <CashClosingPage />
            </ProtectedRoute>
          }
        />
      </Route>

      <Route 
        path="/history" 
        element={
          <ProtectedRoute action="read" subject="incomes">
            <HistoryClosingPage />
          </ProtectedRoute>
        }
      /> 
    </Routes>
  );
};
