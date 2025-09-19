import { useKeycloak } from "@react-keycloak/web";

export const AuthRouter = () => {
  
  // TEST DE KEYCLOAK
  const { keycloak, initialized } = useKeycloak();

  if (!initialized) {
    return <div>Cargando autenticación...</div>;
  }

  return (
    <div className="flex items-center justify-center h-screen bg-gray-500">
      {!keycloak.authenticated ? (
        <button
          className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
          onClick={() => keycloak.login()}
        >
          Iniciar sesión
        </button>
      ) : (
        <div className="text-center">
          <h1 className="text-3xl">
            Autenticación exitosa con Keycloak <br/>
            Bienvenido {keycloak.tokenParsed?.preferred_username}
          </h1>
          <button
            className="bg-red-500 hover:bg-red-700 text-white mt-5 font-bold py-2 px-4 rounded"
            onClick={() => keycloak.logout()}
          >
            Cerrar sesión
          </button>
        </div>
      )}
    </div>
  );
};
