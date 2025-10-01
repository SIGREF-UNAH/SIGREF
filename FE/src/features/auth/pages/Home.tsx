import { useKeycloak } from "@react-keycloak/web";

export const Home = () => {
  const { keycloak } = useKeycloak();

  return (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        height: "100vh",
        background: "linear-gradient(135deg, #3A6EA5, #5A8BD7)",
      }}
    >

      <div
        style={{
          backgroundColor: "#fff",
          padding: "40px 35px",
          borderRadius: "14px",
          boxShadow: "0 6px 20px rgba(0, 0, 0, 0.1)",
          width: "400px",
          textAlign: "center",
        }}
      >
        <h1
          style={{
            fontSize: "24px",
            fontWeight: "bold",
            color: "#333",
            marginBottom: "15px",
          }}
        >
          Autenticación exitosa
        </h1>
        <p style={{ fontSize: "18px", color: "#444", marginBottom: "25px" }}>
          Bienvenido{" "}
          <span style={{ fontWeight: "bold" }}>
            {keycloak.tokenParsed?.preferred_username}
          </span>
        </p>
        <button
          onClick={() => keycloak.logout()}
          style={{
            width: "100%",
            padding: "12px",
            backgroundColor: "#d9534f",
            color: "#fff",
            border: "none",
            borderRadius: "8px",
            fontSize: "16px",
            fontWeight: 500,
            cursor: "pointer",
            transition: "background 0.3s ease",
          }}
          onMouseOver={(e) =>
            (e.currentTarget.style.backgroundColor = "#b52b27")
          }
          onMouseOut={(e) =>
            (e.currentTarget.style.backgroundColor = "#d9534f")
          }
        >
          Cerrar sesión
        </button>
      </div>

    </div>
  );
};
