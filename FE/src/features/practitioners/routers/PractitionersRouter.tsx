import { Navigate, Route, Routes } from "react-router";
import { CreatePractitionerPage, PractitionersListPage, UpdatePractitionerPage } from "../pages";
import PractitionerDetailsPage from "../pages/PractitionerDetailsPage";
import { ProtectedRoute } from "../../../shared/components";

export const PractitionersRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      <Route path="/list" element={
        <ProtectedRoute action="read" subject="practitioners">
          <PractitionersListPage />
        </ProtectedRoute>
        }
      />

      <Route path="/create" element={
        <ProtectedRoute action="create" subject="practitioners">
          <CreatePractitionerPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/update/:id" element={
        <ProtectedRoute action="update" subject="practitioners">
          <UpdatePractitionerPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/details/:id" element={
        <ProtectedRoute action="read" subject="practitioners">
          <PractitionerDetailsPage />
        </ProtectedRoute>
        } 
      />
    </Routes>
  );
};