import { Navigate, Route, Routes } from "react-router";
import LocationDetailsPage from "../pages/LocationDetailsPage";
import CreateLocationPage from "../pages/CreateLocationPage";
import LocationListPage from "../pages/LocationListPage";
import UpdateLocationPage from "../pages/UpdateLocationPage";

export const LocationsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<LocationListPage />} />
      <Route path="/create" element={<CreateLocationPage />} />  
      <Route path="/update/:id" element={<UpdateLocationPage />} />
      <Route path="/details/:id" element={<LocationDetailsPage />} />    
    </Routes>
  );
};
