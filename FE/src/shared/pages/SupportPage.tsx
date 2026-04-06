import { Card, Row, Col, Typography, Space, Button, Spin } from "antd";
import { GithubOutlined, MailOutlined, PhoneOutlined } from "@ant-design/icons";
import { teamMembers, type TeamMemberWithAvatar } from "../store";
import { useState, useEffect } from "react";

const { Title, Text } = Typography;

export const SupportPage = () => {
  const [membersWithAvatars, setMembersWithAvatars] = useState<TeamMemberWithAvatar[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Función para obtener el avatar de GitHub
  const fetchGitHubAvatar = async (username: string): Promise<string> => {
    try {
      const response = await fetch(`https://api.github.com/users/${username}`);
      if (!response.ok) {
        throw new Error(`Error: ${response.status}`);
      }
      const userData = await response.json();
      return userData.avatar_url;
    } catch (error) {
      console.error(`Error al cargar el avatar de ${username}:`, error);
      return ""; 
    }
  };

  // Efecto para cargar los avatares cuando el componente se monta
  useEffect(() => {
    const loadAvatars = async () => {
      setIsLoading(true);
      try {
        const updatedMembers = await Promise.all(
          teamMembers.map(async (member) => {
            if (member.github?.username) {
              const avatarUrl = await fetchGitHubAvatar(member.github.username);
              return { ...member, avatarUrl };
            }
            return { ...member, avatarUrl: "" };
          })
        );
        setMembersWithAvatars(updatedMembers);
        setIsLoading(false);
      } catch (error) {
        console.error("Error al cargar los avatares:", error);
        setIsLoading(false);
      }
    };

    loadAvatars();
  }, []);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div>
      <div className="mb-8 text-center">
        <Title level={2} className="mb-2">
          Equipo de Soporte
        </Title>
        <Text type="secondary">
          Contacta a nuestro equipo de desarrollo para cualquier consulta o
          asistencia
        </Text>
      </div>

      <Row gutter={[16, 16]}>
        {membersWithAvatars.map((member) => (
          <Col xs={24} sm={12} lg={6} key={member.id}>
            <Card className="primary-card">
              <Space direction="vertical" size="middle" className="w-full">
                <div className="text-center">
                  <div className="w-20 h-20 rounded-full bg-primary text-white flex items-center justify-center mx-auto mb-4 overflow-hidden">
                    {member.avatarUrl ? (
                      <img 
                        src={member.avatarUrl} 
                        alt={member.name} 
                        className="w-full h-full object-cover"
                        onError={(e) => {
                          // Fallback si la imagen no carga
                          const target = e.target as HTMLImageElement;
                          target.style.display = 'none';
                        }}
                      />
                    ) : (
                      <div className="w-full h-full bg-gray-300 flex items-center justify-center rounded-full">
                        <span className="text-gray-600 font-semibold">
                          {member.name.split(' ').map(n => n[0]).join('').toUpperCase()}
                        </span>
                      </div>
                    )}
                  </div>
                  <Title level={4} className="mb-1">
                    {member.name}
                  </Title>
                  <Text type="secondary">{member.role}</Text>
                </div>

                <div className="flex justify-center">
                  <Space size="middle">
                    {/* Botón de WhatsApp */}
                    <a 
                      href={`https://wa.me/${member.phone.replace(/\D/g, '')}`} 
                      target="_blank" 
                      rel="noopener noreferrer"
                    >
                      <Button 
                        type="primary" 
                        shape="circle" 
                        icon={<PhoneOutlined />} 
                        style={{ backgroundColor: '#25D366', borderColor: '#25D366' }}
                        className="hover:opacity-90"
                      />
                    </a>

                    {/* Botón de Email */}
                    <a 
                      href={`mailto:${member.email}`}
                    >
                      <Button 
                        type="primary" 
                        shape="circle" 
                        icon={<MailOutlined />} 
                        style={{ backgroundColor: '#1890ff', borderColor: '#1890ff' }}
                        className="hover:opacity-90"
                      />
                    </a>

                    {/* Botón de GitHub */}
                    {member.github && (
                      <a 
                        href={`https://github.com/${member.github.username}`} 
                        target="_blank" 
                        rel="noopener noreferrer"
                      >
                        <Button 
                          type="primary" 
                          shape="circle" 
                          icon={<GithubOutlined />} 
                          style={{ backgroundColor: '#333', borderColor: '#333' }}
                          className="hover:opacity-90"
                        />
                      </a>
                    )}
                  </Space>
                </div>
              </Space>
            </Card>
          </Col>
        ))}
      </Row>
    </div>
  );
};