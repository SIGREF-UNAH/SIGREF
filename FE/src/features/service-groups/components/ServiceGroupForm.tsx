import type { CreateServiceGroupDto, ServiceGroupDto } from "../../../api/models";
import {
    ProForm,
    ProFormText,
    ProFormSelect,
} from "@ant-design/pro-components";
import { Tag, Space, Typography, Badge, Alert } from "antd";
import {
    MedicineBoxOutlined,
    EnvironmentOutlined,
    InfoCircleOutlined,
    CheckCircleOutlined
} from "@ant-design/icons";
import { useHealthcareSearch } from "../hooks/useHealthcareSearch";
import { useLocationSearch } from "../../../shared/hooks/useLocationSearch";
import { ListStatus, getListStatusOptions } from "../../../shared/utils";

const { Text } = Typography;

interface ServiceGroupFormProps {
    initialValues?: Partial<ServiceGroupDto>;
    onFinish: (values: CreateServiceGroupDto) => Promise<void>;
    submitButtonText?: string;
    isPending?: boolean;
    onCancel: () => void;
}

export const ServiceGroupForm = ({
    initialValues,
    submitButtonText = "Crear paquete",
    isPending = false,
    onFinish,
    onCancel,
}: ServiceGroupFormProps) => {
    const { options: healthcareOptions, searchHealthcares } = useHealthcareSearch();
    const { options: locationOptions, searchLocations } = useLocationSearch();

    // Crear opciones iniciales desde los valores cargados
    const initialHealthcareOptions = initialValues?.items?.map(item => ({
        label: item.name || "",
        value: item.id || "",
    })) || [];

    const initialLocationOptions = initialValues?.locations?.map(loc => ({
        label: loc.name || "",
        value: loc.id || "",
    })) || [];

    // Combinar opciones iniciales con las de búsqueda (evitando duplicados)
    const allHealthcareOptions = [...initialHealthcareOptions];
    healthcareOptions.forEach(opt => {
        if (!allHealthcareOptions.find(o => o.value === opt.value)) {
            allHealthcareOptions.push(opt);
        }
    });

    const allLocationOptions = [...initialLocationOptions];
    locationOptions.forEach(opt => {
        if (!allLocationOptions.find(o => o.value === opt.value)) {
            allLocationOptions.push(opt);
        }
    });

    const handleFinish = async (values: any) => {
        const serviceGroupData: CreateServiceGroupDto = {
            title: values.title,
            status: values.status,
            code: {
                coding: [
                    {
                        code: values.abbreviation,
                        display: values.title,
                    },
                ],
            },
            healthcareServiceIds: values.healthcareServiceIds || [],
            locationIds: values.locationIds || [],
        };

        await onFinish(serviceGroupData);
    };

    const abbreviation = initialValues?.code?.coding?.[0]?.code || "";
    const healthcareServiceIds = initialValues?.items?.map(item => item.id).filter(Boolean) || [];
    const locationIds = initialValues?.locations?.map(loc => loc.id).filter(Boolean) || [];

    return (
        <ProForm
            layout="vertical"
            onFinish={handleFinish}
            submitter={{
                render: (_) => (
                    <div className="flex justify-end gap-3 mt-4">
                        <button
                            type="button"
                            onClick={onCancel}
                            disabled={isPending}
                            className="px-8 py-2 text-white bg-red-500 hover:bg-red-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Cancelar
                        </button>
                        <button
                            type="submit"
                            disabled={isPending}
                            className="px-8 py-2 text-white bg-green-500 hover:bg-green-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {isPending ? "Procesando..." : submitButtonText}
                        </button>
                    </div>
                ),
            }}
            grid
            initialValues={{
                title: initialValues?.title || "",
                abbreviation: abbreviation,
                status: initialValues?.status || ListStatus.Current,
                healthcareServiceIds: healthcareServiceIds,
                locationIds: locationIds,
            }}
        >
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 w-full">
                <div className="space-y-4 bg-gray-50 p-5 rounded-lg border border-gray-200">
                    <div className="mb-4">
                        <Text strong className="text-gray-700 text-base">📋 Información General</Text>
                    </div>
                    
                    <ProFormText
                        name="title"
                        label="Nombre del Paquete"
                        placeholder="Ej. Paquete Básico de Salud"
                        rules={[
                            { required: true, message: "El nombre es requerido" },
                            { min: 3, message: "El nombre debe tener al menos 3 caracteres" },
                        ]}
                        fieldProps={{
                            size: "large",
                            disabled: isPending,
                        }}
                    />

                    <ProFormText
                        name="abbreviation"
                        label="Abreviatura"
                        placeholder="Ej. PBS"
                        rules={[
                            { required: true, message: "La abreviatura es requerida" },
                            { max: 10, message: "La abreviatura no puede tener más de 10 caracteres" },
                        ]}
                        fieldProps={{
                            size: "large",
                            disabled: isPending,
                        }}
                    />

                    <ProFormSelect
                        name="status"
                        label="Estado"
                        placeholder="Seleccione un estado"
                        options={getListStatusOptions()}
                        rules={[
                            { required: true, message: "El estado es requerido" },
                        ]}
                        fieldProps={{
                            size: "large",
                            disabled: isPending,
                        }}
                    />
                </div>

                <div className="space-y-6">
                    {/* Servicios de Salud */}
                    <div className="bg-blue-50 p-4 rounded-lg border border-blue-200">
                        <Space direction="vertical" size="middle" className="w-full">
                            <div className="flex items-center gap-2">
                                <MedicineBoxOutlined className="text-blue-600 text-lg" />
                                <Text strong className="text-blue-900">Servicios de Salud</Text>
                                <Badge 
                                    count={healthcareServiceIds.length} 
                                    showZero 
                                    style={{ backgroundColor: '#1890ff' }}
                                />
                            </div>
                            
                            <Alert
                                message="Haz clic para ver servicios disponibles o busca escribiendo"
                                type="info"
                                icon={<InfoCircleOutlined />}
                                showIcon
                                closable
                                className="text-xs"
                            />

                            <ProFormSelect
                                name="healthcareServiceIds"
                                placeholder="🔍 Buscar servicios... (Ej. Consulta, Laboratorio, Rayos X)"
                                mode="multiple"
                                showSearch
                                rules={[
                                    { required: true, message: "Debe seleccionar al menos un servicio" },
                                ]}
                                fieldProps={{
                                    size: "large",
                                    loading: isPending,
                                    filterOption: false,
                                    onSearch: searchHealthcares,
                                    onFocus: () => searchHealthcares(''),
                                    options: allHealthcareOptions,
                                    maxTagCount: 'responsive',
                                    maxTagPlaceholder: (omittedValues) => (
                                        <Tag color="blue">+{omittedValues.length} más</Tag>
                                    ),
                                    tagRender: (props) => {
                                        const { label, closable, onClose } = props;
                                        return (
                                            <Tag
                                                color="blue"
                                                closable={closable}
                                                onClose={onClose}
                                                style={{ marginRight: 3, fontSize: '13px', padding: '2px 8px' }}
                                                icon={<CheckCircleOutlined />}
                                            >
                                                {label}
                                            </Tag>
                                        );
                                    },
                                    notFoundContent: (
                                        <div className="text-center py-4 text-gray-500">
                                            <MedicineBoxOutlined className="text-3xl mb-2" />
                                            <div>Busca servicios escribiendo su nombre</div>
                                        </div>
                                    ),
                                }}
                                debounceTime={300}
                            />
                        </Space>
                    </div>

                    {/* Ubicaciones */}
                    <div className="bg-green-50 p-4 rounded-lg border border-green-200">
                        <Space direction="vertical" size="middle" className="w-full">
                            <div className="flex items-center gap-2">
                                <EnvironmentOutlined className="text-green-600 text-lg" />
                                <Text strong className="text-green-900">Áreas Asistenciales</Text>
                                <Badge 
                                    count={locationIds.length} 
                                    showZero 
                                    style={{ backgroundColor: '#52c41a' }}
                                />
                            </div>

                            <Alert
                                message="Haz clic para ver ubicaciones o busca escribiendo el nombre"
                                type="success"
                                icon={<InfoCircleOutlined />}
                                showIcon
                                closable
                                className="text-xs"
                            />

                            <ProFormSelect
                                name="locationIds"
                                placeholder="📍 Buscar ubicaciones... (Ej. Emergencia, Consulta Externa)"
                                mode="multiple"
                                showSearch
                                rules={[
                                    { required: true, message: "Debe seleccionar al menos una ubicación" },
                                ]}
                                fieldProps={{
                                    size: "large",
                                    loading: isPending,
                                    filterOption: false,
                                    onSearch: searchLocations,
                                    onFocus: () => searchLocations(''),
                                    options: allLocationOptions,
                                    maxTagCount: 'responsive',
                                    maxTagPlaceholder: (omittedValues) => (
                                        <Tag color="green">+{omittedValues.length} más</Tag>
                                    ),
                                    tagRender: (props) => {
                                        const { label, closable, onClose } = props;
                                        return (
                                            <Tag
                                                color="green"
                                                closable={closable}
                                                onClose={onClose}
                                                style={{ marginRight: 3, fontSize: '13px', padding: '2px 8px' }}
                                                icon={<CheckCircleOutlined />}
                                            >
                                                {label}
                                            </Tag>
                                        );
                                    },
                                    notFoundContent: (
                                        <div className="text-center py-4 text-gray-500">
                                            <EnvironmentOutlined className="text-3xl mb-2" />
                                            <div>Busca ubicaciones escribiendo su nombre</div>
                                        </div>
                                    ),
                                }}
                                debounceTime={300}
                            />
                        </Space>
                    </div>
                </div>
            </div>

            {/* Resumen Visual */}
            {(healthcareServiceIds.length > 0 || locationIds.length > 0) && (
                <div className="mt-6 p-4 bg-gradient-to-r from-blue-50 to-green-50 rounded-lg border border-blue-200">
                    <Space direction="vertical" size="small" className="w-full">
                        <div className="flex items-center gap-2 mb-2">
                            <InfoCircleOutlined className="text-blue-600" />
                            <Text strong className="text-gray-700">Resumen del Paquete</Text>
                        </div>
                        
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div className="flex items-start gap-2">
                                <MedicineBoxOutlined className="text-blue-500 mt-1" />
                                <div>
                                    <Text type="secondary" className="text-xs block">Servicios seleccionados:</Text>
                                    <Badge 
                                        count={healthcareServiceIds.length} 
                                        showZero
                                        style={{ backgroundColor: '#1890ff' }}
                                    />
                                </div>
                            </div>
                            
                            <div className="flex items-start gap-2">
                                <EnvironmentOutlined className="text-green-500 mt-1" />
                                <div>
                                    <Text type="secondary" className="text-xs block">Ubicaciones seleccionadas:</Text>
                                    <Badge 
                                        count={locationIds.length} 
                                        showZero
                                        style={{ backgroundColor: '#52c41a' }}
                                    />
                                </div>
                            </div>
                        </div>
                    </Space>
                </div>
            )}
        </ProForm>
    );
};
