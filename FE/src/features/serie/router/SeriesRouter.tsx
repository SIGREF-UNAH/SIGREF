import { Navigate, Route, Routes } from "react-router";
import { ProtectedRoute } from "../../../shared/components";
import { SeriesCreatePage, SeriesListPage } from "../pages";

export const SeriesRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="list" replace />} />

      {/* Ruta para crear una nueva serie */}
      <Route path="/create" element={
        <ProtectedRoute action="create" subject="series">
          <SeriesCreatePage/>
        </ProtectedRoute>
        } 
      /> 
      
      {/* Ruta para listar las series */}
      <Route path="/list" element={
        <ProtectedRoute action="read" subject="series">
          <SeriesListPage />
        </ProtectedRoute>
        } 
      /> 


    
    </Routes>
  );
};