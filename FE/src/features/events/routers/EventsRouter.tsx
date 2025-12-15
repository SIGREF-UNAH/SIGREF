import { Navigate, Route, Routes } from "react-router";
import { EventHistoryPage } from "../pages";
import { ProtectedRoute } from "../../../shared/components";

export const EventsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      <Route path="/list" element={
        <ProtectedRoute action="read" subject="events">
          <EventHistoryPage/>
        </ProtectedRoute>
        }
      />
    </Routes>
  );
};