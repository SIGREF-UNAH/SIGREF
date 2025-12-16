import { Navigate, Route, Routes } from "react-router";
import { ProtectedRoute } from "../../../shared/components";
import OpenCashierSessionPage from "../pages/OpenCashierSessionPage";
import { CashierRouteGuard } from "../components";

export const CashierSessionsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="open-session" replace />} />

      <Route element={<CashierRouteGuard requiresActiveSession={false} requiresCashierRole={true} />}>
        <Route
          path="/open-session"
          element={
            <ProtectedRoute action="create" subject="cashier-sessions">
              <OpenCashierSessionPage />
            </ProtectedRoute>
          }
        />
      </Route>
    </Routes>
  );
};
