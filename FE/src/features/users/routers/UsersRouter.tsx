import { Navigate, Route, Routes } from "react-router"
import CreateUsersPage from "../pages/CreateUsersPage"
import { ProtectedRoute } from "../../../shared/components"

export const UsersRouter = () => {
  return (
    <Routes>
      <Route path="*" element={<Navigate to="create" replace />} />

      <Route path="/create" element={
        <ProtectedRoute action="create" subject="users">
          <CreateUsersPage />
        </ProtectedRoute>
        } 
      />
    </Routes>
  )
}

