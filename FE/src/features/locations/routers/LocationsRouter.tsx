import { Navigate, Route, Routes } from "react-router";

import EditLocationPage from "../pages/EditLocationPage";

import LocationDetailsPage from "../pages/LocationDetailsPage";
import CreateLocationPage from "../pages/CreateLocationPage";
import LocationListPage from "../pages/LocationListPage";

export const LocationsRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />
      <Route path="/list" element={<LocationListPage />} />
      <Route path="/create" element={<CreateLocationPage />} />  

      <Route path="/update/:id" element={<EditLocationPage />} />
      <Route path="/details/:id" element={<LocationDetailsPage />} />    
    </Routes>
  );
};
