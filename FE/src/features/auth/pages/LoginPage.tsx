import { LoginForm, ProFormText } from '@ant-design/pro-components';
import { Button, Typography } from 'antd';
import React from 'react';
import '@ant-design/v5-patch-for-react-19';
import { Row, Col } from 'antd';
import { useAuthLogin } from '../hooks/useAuthLogin';

const { Title } = Typography;

const LoginPage: React.FC = () => {
  const { login, loading } = useAuthLogin();

  return (
    <div style={{ display: 'flex', minHeight: '100vh', width: '100%' }}>
      <Row style={{ flex: 1, margin: 0 }}>
        <Col
          span={12}
          style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '100vh',
          }}
        >
          <h1
            style={{
              fontSize: '64px',
              fontWeight: 'bold',
              color: '#000',
            }}
          >
            SIGREF
          </h1>
        </Col>

        <Col
          span={12}
          style={{
            minHeight: '100vh',
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'center',
            alignItems: 'center',
            backgroundColor: '#3A6EA5',
            margin: 0,
            padding: 0,
          }}
        >
            <div style={{ marginBottom: 20 }}>
              <Title
                level={1}
                style={{
                textAlign: 'center',
                marginBottom: 10,
                color: '#FFFFFF',
                fontWeight: 600,
                letterSpacing: '0.5px',
              }}
            >
              Inicia Sesión
            </Title>
          </div>
        
          <div
            style={{
              backgroundColor: '#fff',
              padding: '40px 35px',
              borderRadius: '14px',
              boxShadow: '0 6px 20px rgba(0, 0, 0, 0.1)',
              width: 380,
              maxWidth: '90%',
            }}
          >
            
            <LoginForm
              onFinish={login}
              contentStyle={{
                boxShadow: 'none',
                padding: 0,
                margin: 0,
                minHeight: 'unset',
                overflow: 'visible',
                width: '100%',
                minWidth: 'unset',
              }}
              style={{
                margin: 0,
                padding: 0,
                width: '100%',
              }}
              submitter={{
                searchConfig: {
                  submitText: 'Ingresar',
                },
                render: () => (
                  <Button
                    type='primary'
                    htmlType='submit'
                    loading={loading}
                    block
                    style={{
                      backgroundColor: '#3A6EA5',
                      borderColor: '#3A6EA5',
                      borderRadius: 8,
                      height: 42,
                      fontWeight: 500,
                      fontSize: '15px',
                      color: '#fff',
                      marginTop: 10,
                    }}
                  >
                    Inicia Sesión
                  </Button>
                ),
              }}
            >
              <ProFormText
                name='usuario'
                placeholder='Nombre de usuario:'
                rules={[{ required: true, message: 'Por favor ingrese su usuario' }]}
                fieldProps={{
                  style: { height: 42 },
                }}
              />

              <ProFormText.Password
                name='password'
                placeholder='********'
                rules={[{ required: true, message: 'Por favor ingrese su contraseña' }]}
                fieldProps={{
                  style: { height: 42 },
                }}
              />
            </LoginForm>
          </div>
        </Col>
      </Row>
    </div>
  );
};

export default LoginPage;

