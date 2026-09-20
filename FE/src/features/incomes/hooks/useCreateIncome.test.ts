import { describe, expect, it } from 'vitest'
import { buildInvoiceItems } from './useCreateIncome'

describe('buildInvoiceItems', () => {
  it('normaliza un servicio individual', () => {
    expect(
      buildInvoiceItems({
        id: 'service-1',
        nombre: 'Consulta',
        precio: 75,
        tipo: 'servicio',
      }),
    ).toEqual([
      {
        serviceId: 'service-1',
        nameService: 'Consulta',
        quantity: 1,
        unitPrice: 75,
      },
    ])
  })

  it('usa el servicio de nivel paquete cuando faltan sus items', () => {
    expect(
      buildInvoiceItems({
        id: 'package-1',
        nombre: 'Paquete básico',
        precio: 100,
        tipo: 'paquete',
      }),
    ).toEqual([
      {
        serviceId: 'package-1',
        nameService: 'Paquete básico',
        quantity: 1,
        unitPrice: 100,
      },
    ])
  })

  it('convierte los servicios de un paquete en items de factura tipados', () => {
    const items = buildInvoiceItems({
      id: 'package-1',
      nombre: 'Paquete básico',
      precio: 100,
      tipo: 'paquete',
      items: [
        {
          id: 'service-1',
          name: 'Consulta',
          price: 75,
        },
        {
          serviceId: 'service-2',
          nameService: 'Examen',
          unitPrice: 25,
          quantity: 2,
        },
      ],
    })

    expect(items).toEqual([
      {
        serviceId: 'service-1',
        nameService: 'Consulta',
        quantity: 1,
        unitPrice: 75,
      },
      {
        serviceId: 'service-2',
        nameService: 'Examen',
        quantity: 2,
        unitPrice: 25,
      },
    ])
  })
})
