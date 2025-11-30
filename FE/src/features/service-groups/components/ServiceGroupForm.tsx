import { useState, useEffect } from "react";
import type {
    CreateServiceGroupDto,
    ServiceGroupDto,
    HealthcareDto,
    LocationDto,
    PaginationDto,
} from "../../../api/models";
import { ProForm, ProFormText, ProFormSelect } from "@ant-design/pro-components";
import { Tag, Typography, Badge, Space, Button } from "antd";
import type { ColumnsType } from "antd/es/table";
import {
    MedicineBoxOutlined,
    EnvironmentOutlined,
    InfoCircleOutlined,
} from "@ant-design/icons";
import { ListStatus, getListStatusOptions } from "../../../shared/utils";
import { SelectionTable } from "../../../shared/components/SelectionTable";

const { Text } = Typography;

interface ServiceGroupFormProps {
    initialValues?: Partial<ServiceGroupDto>;
    healthcares: HealthcareDto[];
    locations: LocationDto[];
    healthcarePagination?: PaginationDto;
    locationPagination?: PaginationDto;
    isLoadingHealthcares?: boolean;
    isLoadingLocations?: boolean;
    isFetchingHealthcares?: boolean;
    isFetchingLocations?: boolean;
    onHealthcarePageChange?: (page: number, pageSize: number) => void;
    onLocationPageChange?: (page: number, pageSize: number) => void;
    onHealthcareSearch?: (search: string) => void;
    onLocationSearch?: (search: string) => void;
    onFinish: (values: CreateServiceGroupDto) => Promise<void>;
    submitButtonText?: string;
    isPending?: boolean;
    onCancel: () => void;
}

export const ServiceGroupForm = ({
    initialValues,
    healthcares,
    locations,
    healthcarePagination,
    locationPagination,
    isLoadingHealthcares = false,
    isLoadingLocations = false,
    isFetchingHealthcares = false,
    isFetchingLocations = false,
    onHealthcarePageChange,
    onLocationPageChange,
    onHealthcareSearch,
    onLocationSearch,
    submitButtonText = "Crear paquete",
    isPending = false,
    onFinish,
    onCancel,
}: ServiceGroupFormProps) => {
    // Estado para servicios de salud seleccionados
    const [selectedHealthcareKeys, setSelectedHealthcareKeys] = useState<React.Key[]>([]);

    // Estado para ubicaciones seleccionadas
    const [selectedLocationKeys, setSelectedLocationKeys] = useState<React.Key[]>([]);

    // Inicializar selecciones desde initialValues
    useEffect(() => {
        if (initialValues?.items && initialValues.items.length > 0) {
            const healthcareIds = initialValues.items
                .map((item) => item.id)
                .filter(Boolean) as string[];
            setSelectedHealthcareKeys(healthcareIds);

            // Los servicios seleccionados se cargarán automáticamente al mostrar la tabla
        }
    }, [initialValues?.items, healthcares]);

    useEffect(() => {
        if (initialValues?.locations && initialValues.locations.length > 0) {
            const locationIds = initialValues.locations
                .map((loc) => loc.id)
                .filter(Boolean) as string[];
            setSelectedLocationKeys(locationIds);

            // Las ubicaciones seleccionadas se cargarán automáticamente al mostrar la tabla
        }
    }, [initialValues?.locations, locations]);

    // Columnas para tabla de servicios de salud
    const healthcareColumns: ColumnsType<HealthcareDto> = [
        {
            title: "Nombre",
            dataIndex: "name",
            key: "name",
            ellipsis: true,
        },
        {
            title: "Estado",
            dataIndex: "active",
            key: "active",
            width: 100,
            render: (active: boolean) => (
                <Tag color={active ? "green" : "red"}>
                    {active ? "✓ Activo" : "✗ Inactivo"}
                </Tag>
            ),
        },
    ];

    // Columnas para tabla de ubicaciones
    const locationColumns: ColumnsType<LocationDto> = [
        {
            title: "Nombre",
            dataIndex: "name",
            key: "name",
            ellipsis: true,
        },
        {
            title: "Estado",
            dataIndex: "status",
            key: "status",
            width: 120,
            render: (status: string) => {
                const normalized = status?.toLowerCase();
                if (normalized === "active") return <Tag color="green">✓ Activo</Tag>;
                if (normalized === "suspended")
                    return <Tag color="orange">⚠︎ Suspendido</Tag>;
                if (normalized === "inactive") return <Tag color="red">✗ Inactivo</Tag>;
                return <Tag>{status}</Tag>;
            },
        },
    ];

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
            healthcareServiceIds: selectedHealthcareKeys as string[],
            locationIds: selectedLocationKeys as string[],
            description: values.description || null,
        };

        await onFinish(serviceGroupData);
    };

    const abbreviation = initialValues?.code?.coding?.[0]?.code || "";

    return (
        <ProForm
            layout="vertical"
            onFinish={handleFinish}
            submitter={{
                render: (_) => (
                    <div className="flex justify-end gap-3 mt-4">
                        <Button
                            onClick={onCancel}
                            disabled={isPending}
                        >
                            Cancelar
                        </Button>
                        <Button
                            disabled={
                                isPending ||
                                selectedHealthcareKeys.length === 0 ||
                                selectedLocationKeys.length === 0
                            }
                            type="primary"
                        >
                            {isPending ? "Procesando..." : submitButtonText}
                        </Button>
                    </div>
                ),
            }}
            initialValues={{
                title: initialValues?.title || "",
                abbreviation: abbreviation,
                status: initialValues?.status || ListStatus.Current,
                description: initialValues?.description || "",
            }}
        >
            {/* Información General */}
            <div className="bg-white p-6 rounded-lg border border-gray-200 mb-6">
                <div className="mb-5">
                    <Text strong className="text-gray-800 text-lg">
                        📋 Información General
                    </Text>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                    <ProFormText
                        name="title"
                        label="Nombre del Paquete"
                        placeholder="Ej. Paquete Básico de Salud"
                        rules={[
                            { required: true, message: "El nombre es requerido" },
                            { min: 3, message: "El nombre debe tener al menos 3 caracteres" },
                        ]}
                        fieldProps={{
                            disabled: isPending,
                        }}
                    />

                    <ProFormText
                        name="abbreviation"
                        label="Abreviatura"
                        placeholder="Ej. PBS"
                        rules={[
                            { required: true, message: "La abreviatura es requerida" },
                            {
                                max: 10,
                                message: "La abreviatura no puede tener más de 10 caracteres",
                            },
                        ]}
                        fieldProps={{
                            disabled: isPending,
                        }}
                    />

                    <ProFormSelect
                        name="status"
                        label="Estado"
                        placeholder="Seleccione un estado"
                        options={getListStatusOptions()}
                        rules={[{ required: true, message: "El estado es requerido" }]}
                        fieldProps={{
                            disabled: isPending,
                        }}
                    />
                </div>

                <ProFormText
                    name="description"
                    label="Descripción"
                    placeholder="Ej. Paquete que incluye servicios básicos de atención primaria"
                    fieldProps={{
                        disabled: isPending,
                    }}
                />
            </div>

            {/* Tablas de Selección */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Servicios de Salud */}
                <div className="bg-blue-50 p-5 rounded-lg border border-blue-200">
                    <Space direction="vertical" size="small" className="w-full">
                        <div className="flex items-center gap-2 mb-3">
                            <MedicineBoxOutlined className="text-blue-600 text-lg" />
                            <Text strong className="text-blue-900 text-base">
                                Servicios de Salud
                            </Text>
                            <Badge
                                count={selectedHealthcareKeys.length}
                                showZero
                                style={{ backgroundColor: "#1890ff" }}
                            />
                        </div>

                        <SelectionTable
                            dataSource={healthcares}
                            columns={healthcareColumns}
                            rowKey="id"
                            selectedRowKeys={selectedHealthcareKeys}
                            onSelectionChange={(keys) => {
                                setSelectedHealthcareKeys(keys);
                            }}
                            searchPlaceholder="Buscar servicios..."
                            onSearch={onHealthcareSearch}
                            loading={isLoadingHealthcares || isFetchingHealthcares}
                            disabled={isPending}
                            pagination={
                                healthcarePagination
                                    ? {
                                        current: healthcarePagination.currentPage || 1,
                                        pageSize: healthcarePagination.pageSize || 15,
                                        total: healthcarePagination.totalItems || 0,
                                        showSizeChanger: true,
                                        showTotal: (total, range) =>
                                            `${range[0]}-${range[1]} de ${total}`,
                                        onChange: (page, pageSize) => {
                                            onHealthcarePageChange?.(page, pageSize);
                                        },
                                        pageSizeOptions: ["10", "15", "20", "30"],
                                        size: "small",
                                    }
                                    : false
                            }
                            emptyText="No hay servicios disponibles"
                        />

                        {selectedHealthcareKeys.length === 0 && (
                            <div className="text-red-600 text-xs mt-1 flex items-center gap-1">
                                <InfoCircleOutlined />
                                <span>Debe seleccionar al menos un servicio</span>
                            </div>
                        )}
                    </Space>
                </div>

                {/* Ubicaciones */}
                <div className="bg-green-50 p-5 rounded-lg border border-green-200">
                    <Space direction="vertical" size="small" className="w-full">
                        <div className="flex items-center gap-2 mb-3">
                            <EnvironmentOutlined className="text-green-600 text-lg" />
                            <Text strong className="text-green-900 text-base">
                                Áreas Asistenciales
                            </Text>
                            <Badge
                                count={selectedLocationKeys.length}
                                showZero
                                style={{ backgroundColor: "#52c41a" }}
                            />
                        </div>

                        <SelectionTable
                            dataSource={locations}
                            columns={locationColumns}
                            rowKey="id"
                            selectedRowKeys={selectedLocationKeys}
                            onSelectionChange={(keys) => {
                                setSelectedLocationKeys(keys);
                            }}
                            searchPlaceholder="Buscar ubicaciones..."
                            onSearch={onLocationSearch}
                            loading={isLoadingLocations || isFetchingLocations}
                            disabled={isPending}
                            pagination={
                                locationPagination
                                    ? {
                                        current: locationPagination.currentPage || 1,
                                        pageSize: locationPagination.pageSize || 15,
                                        total: locationPagination.totalItems || 0,
                                        showSizeChanger: true,
                                        showTotal: (total, range) =>
                                            `${range[0]}-${range[1]} de ${total}`,
                                        onChange: (page, pageSize) => {
                                            onLocationPageChange?.(page, pageSize);
                                        },
                                        pageSizeOptions: ["10", "15", "20", "30"],
                                        size: "small",
                                    }
                                    : false
                            }
                            emptyText="No hay ubicaciones disponibles"
                        />

                        {selectedLocationKeys.length === 0 && (
                            <div className="text-red-600 text-xs mt-1 flex items-center gap-1">
                                <InfoCircleOutlined />
                                <span>Debe seleccionar al menos una ubicación</span>
                            </div>
                        )}
                    </Space>
                </div>
            </div>
        </ProForm>
    );
};
