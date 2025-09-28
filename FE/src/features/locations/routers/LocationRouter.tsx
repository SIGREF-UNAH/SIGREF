import { Routes, Route } from "react-router";
import EditLocations from "../pages/edit/EditLocation";
 
const LocationRouter = () => {
  return (
    <Routes>
      <Route path="/editlocation" element={<EditLocations />} />
    </Routes>
  );
};

export default LocationRouter;
