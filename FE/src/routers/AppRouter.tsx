import { Route, Routes } from "react-router";
import AuthRouter from "../features/auth/routers/AuthRouter";
import LocationRouter from "../features/locations/routers/LocationRouter";

export const AppRouter = () => {
  return (
    <Routes>
      <Route>   
        <Route path="/*" element={<AuthRouter />} />
        <Route path="/locations/*" element={<LocationRouter/>} />
        </Route>  
    </Routes>
  );
};  
