import { Navigate, Route, Routes } from "react-router";
import EditLocations from "../pages/EditLocation";
import LocationDetailsPage from "../pages/LocationDetailsPage";
import CreateLocationPage from "../pages/CreateLocationPage";
import LocationListPage from "../pages/LocationListPage";

export const LocationsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<LocationListPage />} />
      <Route path="/create" element={<CreateLocationPage />} />  
      <Route path="/update/:id" element={<EditLocations />} />
      <Route path="/details/:id" element={<LocationDetailsPage />} />    
    </Routes>
  );
};
