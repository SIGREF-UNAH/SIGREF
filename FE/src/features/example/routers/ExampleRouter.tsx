import { Navigate, Route, Routes } from "react-router";
import { Example } from "../pages/Example";
import { ProtectedRoute } from "../../../utils/ProtectedRoute";


export const ExampleRouter = () => {
  return (
    <Routes>
        <Route
            path="/"
            element={
            <ProtectedRoute action="read" subject="Event">
                <Example />
            </ProtectedRoute>
            }
        />
        <Route path="*" element={<Navigate to="/example" replace />} />
    </Routes>
  );
};
