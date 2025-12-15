import { Navigate, Route, Routes } from "react-router";
import LocationDetailsPage from "../pages/LocationDetailsPage";
import CreateLocationPage from "../pages/CreateLocationPage";
import LocationListPage from "../pages/LocationListPage";
import UpdateLocationPage from "../pages/UpdateLocationPage";
import { ProtectedRoute } from "../../../shared/components";

export const LocationsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      
      <Route path="/list" element={
        <ProtectedRoute action="read" subject="locations">
          <LocationListPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/create" element={
        <ProtectedRoute action="create" subject="locations">
          <CreateLocationPage />
        </ProtectedRoute>
        } 
      />  

      <Route path="/update/:id" element={
        <ProtectedRoute action="update" subject="locations">
          <UpdateLocationPage />
        </ProtectedRoute>
        } 
      />

      <Route path="/details/:id" element={
        <ProtectedRoute action="read" subject="locations">
          <LocationDetailsPage />
        </ProtectedRoute>
        } 
      />    
    </Routes>
  );
};
