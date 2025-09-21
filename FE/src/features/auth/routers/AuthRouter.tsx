import { LoginForm, ProFormText } from '@ant-design/pro-components';
import { Button, message, Typography  } from 'antd';
import React from 'react';
import '@ant-design/v5-patch-for-react-19';

const { Title } = Typography;

const AuthRouter: React.FC = () => {
  const handleSubmit = async (values: { email: string; password: string }) => {
    try {
      const response = await fetch(
        '/realms/master/account/', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
          },
          body: new URLSearchParams({
            client_id: 'account',
            client_secret: 'account',
            grant_type: 'password',
            username: values.email,
            password: values.password,
          }),
        }
      );

      if (!response.ok) {
        throw new Error('Login failed');
      }

      const data = await response.json();
      console.log('Token recibido:', data);

      // Guardar token en localStorage (o en contexto)
      localStorage.setItem('access_token', data.access_token);

      message.success('Login exitoso');
    } catch (error) {
      console.error(error);
      message.error('Credenciales incorrectas');
    }
  };
return (
  <div
    style={{
      minHeight: "100vh",
      display: "flex",
      justifyContent: "center",
      alignItems: "center",
      backgroundColor: "#ffffff",
      margin: 0,
      padding: 0,
    }}
  >
    <div
      style={{
        backgroundColor: "#fff",
        padding: "40px 35px",
        borderRadius: "14px",
        boxShadow: "0 6px 20px rgba(0, 0, 0, 0.1)",
        width: 380,
        maxWidth: "90%",
      }}
    >
      <Title
        level={3}
        style={{
          textAlign: "center",
          marginBottom: 10,
          color: "#0d6efd",
          fontWeight: 600,
          letterSpacing: "0.5px",
        }}
      >
        SIGREF
      </Title>

      <p
        style={{
          textAlign: "center",
          marginBottom: 30,
          color: "#666",
          fontSize: "15px",
        }}
      >
        Accede con tus credenciales
      </p>

     <LoginForm
  onFinish={handleSubmit}
  contentStyle={{
    boxShadow: "none",
    padding: 0,
    margin: 0,
    minHeight: "unset",
    overflow: "visible",
    width: "100%",        
    minWidth: "unset",     
  }}
  style={{
    margin: 0,
    padding: 0,
    width: "100%",        
  }}
  submitter={{
    searchConfig: {
      submitText: "Ingresar",
    },
    render: () => (
      <Button
        type="primary"
        htmlType="submit"
        block
        style={{
          backgroundColor: "#0d6efd",
          borderColor: "#0d6efd",
          borderRadius: 8,
          height: 42,
          fontWeight: 500,
          fontSize: "15px",
          color: "#fff",
          marginTop: 10,
        }}
      >
        Ingresar
      </Button>
    ),
  }}
>
  <ProFormText
    name="email"
    label="Email"
    placeholder="correo@ejemplo.com"
    rules={[{ required: true, message: "Por favor ingrese su correo" }]}
    fieldProps={{
      style: { height: 42 },  
    }}
  />

  <ProFormText.Password
    name="password"
    label="Contraseña"
    placeholder="********"
    rules={[{ required: true, message: "Por favor ingrese su contraseña" }]}
    fieldProps={{
      style: { height: 42 },  
    }}
  />
</LoginForm>
    </div>
  </div>
);
};

export default AuthRouter;
